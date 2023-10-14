
using System.Numerics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Troublecat.Aseprite;
using Troublecat.Aseprite.Abstractions;
using Troublecat.Aseprite.Chunks;
using Troublecat.Core.Assets.Sprites;
using Troublecat.Core.Diagnostics;
using Troublecat.Core.Geometry;
using Troublecat.Core.Graphics;
using Troublecat.Data.Importers.Aseprite;
using Troublecat.Data.Serialization;
using Troublecat.IO;
using Troublecat.Math;
using Troublecat.Utilities;

namespace Troublecat.Data.Importers;

[ResourceImporter("*.aseprite", "*.ase")]
public class AsepriteImporter : IResourceImporter {
    private readonly ILogger<AsepriteImporter> _logger;
    private readonly ImportConfiguration _configuration;
    private readonly AssetSerializer _serializer;

    public AsepriteImporter(ILogger<AsepriteImporter> logger, IOptions<ImportConfiguration> configuration, AssetSerializer serializer) {
        _logger = logger;
        _configuration = configuration.Value;
        _serializer = serializer;
    }

    private struct SliceInfo {
        public string Name;
        public Vector2 Position;
        public Vector2 Size;
        public Vector2 Pivot;
        public Vector4 Border;
        public int FrameIndex;
    }

    private struct TextureWriteOperation {
        public string FilePath;
        public PixelBucket PixelData;
    }

    public async Task ImportAsync(string resourcePath) {
        using (_logger.BeginScope(new PerfScope())) {
            if (!Paths.Exists(resourcePath)) {
                _logger.LogError($"Resource not found! Does '{resourcePath}' exist?!");
                return;
            }

            var textures = new List<TextureWriteOperation>();

            var packPath = Paths.GetPath(_configuration.ResourcesPackDirectoryAbsolute, "Sprites");
            var fileName = Path.GetFileNameWithoutExtension(resourcePath);
            var filenameWithoutExtension = Path.GetFileNameWithoutExtension(resourcePath);

            var jsonFile = $"{fileName}.json";
            var jsonPackPath = Paths.GetPath(packPath, jsonFile);

            string GetTexturePackPath(string textureName) => Paths.GetPath(packPath, $"{textureName}.png");

            AsepriteImportOptions options = new();
            if (_configuration.Sprites.TryGetValue(fileName, out var opts)) {
                options = opts;
            }
            _logger.LogInformation($"Importing {resourcePath}");

            // load the aseprite file from disk
            var file = AsepriteFile.FromFile(resourcePath);
            int frameCount = file.Header.Frames;

            _logger.LogInformation($"Importing {file.Frames.Count} frames from aseprite file");

            PixelBucket[] frames;
            if (options.ImportStyle != AsepriteImportStyle.FrameAsRow) {
                frames = file.GetFrames();
            } else {
                frames = file.GetLayersAsFrames();
            }

            // create a basic atlas for the sprite
            _logger.LogInformation($"Generating texture atlas...", new { Width = file.Header.Width, Height = file.Header.Height });
            var atlasPack = CreateAtlas(frames, file, options, baseTwo: false);

            textures.Add(new TextureWriteOperation {
               PixelData = atlasPack.Atlas,
               FilePath = GetTexturePackPath(filenameWithoutExtension)
            });

            // extract various masks
            //   - emission
            //   - mask
            var secondaryTextures = new Dictionary<string, PixelBucket>();
            var metadata = new List<AsepriteFileMetadata>();
            foreach(var meta in file.GetMetaData(options.SpritePivot)) {
                if (meta.Type == MetadataType.SecondaryTexture) {
                    var secondaryFrames = file.GetLayerPixels(meta.LayerIndex, meta.Layer, forceVisible: true);
                    var secondaryAtlas = CreateAtlas(secondaryFrames.ToArray(), file, options, baseTwo: false);
                    secondaryTextures.Add(meta.Args[0], secondaryAtlas.Atlas);
                }
                if (meta.Type == MetadataType.Arguments) {
                    var name = meta.Layer.LayerName;
                    foreach (var arg in meta.Args) {
                        if (arg.StartsWith("name=")) {
                            name = arg.Replace("name=", "");
                            break;
                        }
                    }
                    metadata.Add(new AsepriteFileMetadata() { Name = name, Args = meta.Args });
                }
            }

            // import any animations MUST BE DONE AFTER SPRITES ARE IMPORTED
            var animations = ImportAnimations(file, atlasPack.Sprites);

            // create palette swap texture as well
            if (options.EnablePaletteMap &&
                TryGeneratePalette(atlasPack.Atlas, options.PaletteSize.X, options.PaletteSize.Y, out var palette) &&
                TryGeneratePaletteMap(atlasPack.Atlas, palette, out var map, options.PaletteMapColorStep)) {
                    textures.Add(new TextureWriteOperation {
                        FilePath = GetTexturePackPath($"{filenameWithoutExtension}_PaletteTex"),
                        PixelData = palette
                    });
                    textures.Add(new TextureWriteOperation {
                        FilePath = GetTexturePackPath($"{filenameWithoutExtension}_MapTex"),
                        PixelData = map
                    });
            }

            // start saving the assets
            var atlasSavePath = GetTexturePackPath($"{filenameWithoutExtension}");
            var descriptor = new AsepriteAsset() {
                Name = Paths.PathToKey(atlasSavePath),
                Animations = animations,
                AtlasTexturePath = Paths.PathToKey(atlasSavePath),
                Sprites = atlasPack.Sprites,
                Metadata = metadata,
            };

            // assemble each of the secondary textures
            foreach(var entry in secondaryTextures) {
                var secondaryPackPath = GetTexturePackPath($"{filenameWithoutExtension}_{entry.Key}");
                descriptor.SecondaryTexturePaths.Add(entry.Key, Paths.PathToKey(secondaryPackPath));
                textures.Add(new TextureWriteOperation {
                    FilePath = secondaryPackPath,
                    PixelData = new PixelBucket(entry.Value.Width, entry.Value.Height, entry.Value.Pixels)
                });
            }

            // write out the textures
            foreach(var op in textures) {
                _logger.LogInformation($"Writing image {op.FilePath}");
                PngUtilities.WriteImage(op.FilePath, op.PixelData.Width, op.PixelData.Height, op.PixelData.Pixels);
            }

            string json = await _serializer.SerializeAsync(descriptor, false);
            await Files.SaveTextAsync(json, jsonPackPath);
        }
        return;
    }

    private (PixelBucket Atlas, List<Sprite> Sprites) CreateAtlas(PixelBucket[] sprites, AsepriteFile file, AsepriteImportOptions options, bool baseTwo = true) {
        var spriteSize = new Vector2(file.Header.Width, file.Header.Height);
        var cols = sprites.Length;
        var rows = 1;

        if (options.ImportStyle == AsepriteImportStyle.FrameAsRow) {
            rows = sprites.Length;
            cols = 1;
        }

        if (options.ImportStyle == AsepriteImportStyle.TypicalGrid) {
            float spriteCount = sprites.Length;
            int divider = 2;
            var width = cols * spriteSize.X;
            var height = rows * spriteSize.Y;

            while(width > height) {
                cols = (int)System.Math.Ceiling(spriteCount / divider);
                rows = (int)System.Math.Ceiling(spriteCount / cols);

                width = cols * spriteSize.X;
                height = rows * spriteSize.Y;

                if (cols <= 1) {
                    break;
                }

                divider++;
            }
            if (height > width)
                divider -= 2;
            else
                divider -= 1;

            if (divider < 1)
                divider = 1;

            cols = (int)System.Math.Ceiling(spriteCount / divider);
            rows = (int)System.Math.Ceiling(spriteCount / cols);
        }
        return CreateAtlas(sprites, spriteSize, file, cols, rows, options, baseTwo);
    }

    private (PixelBucket Atlas, List<Sprite> Sprites) CreateAtlas(PixelBucket[] sprites, Vector2 spriteSize, AsepriteFile file, int cols, int rows, AsepriteImportOptions options, bool baseTwo = true) {
        int width = cols * (int)spriteSize.X;
        int height = rows * (int)spriteSize.Y;

        if (baseTwo) {
            var baseTwoValue = CalculateNextBaseTwoValue((int)System.Math.Max(width, height));
            width = baseTwoValue;
            height = baseTwoValue;
        }

        var atlas = PixelBucket.CreateTransparent(width, height);
        var index = 0;
        var sliceList = new List<SliceInfo>();

        var importSprites = new List<Sprite>();

        for (var row = 0; row < rows; row++) {
            for (var col = 0; col < cols; col++) {
                var atlasPosition = new Vector2(col * spriteSize.X, atlas.Height - ((row + 1) * spriteSize.Y));
                var spriteRect = new Rectangle(atlasPosition, spriteSize);
                var colorData = FlipTexture(sprites[index], options.EnableMirrorX, options.EnableMirrorY).Pixels;
                colorData = ReplaceMaskToTransparent(options.TransparentColor, colorData);

                atlas.SetPixels(
                    (int)spriteRect.X,
                    (int)spriteRect.Y,
                    (int)spriteRect.Width,
                    (int)spriteRect.Height,
                    colorData);

                if (options.EnableSliceImport) {
                    var frame = file.Frames[index];
                    if (frame != null) {
                        var slices = frame.GetChunks<SliceChunk>();
                        foreach(var slice in slices) {
                            foreach(var sliceEntry in slice.Entries) {
                                var slicePosition = new Vector2(sliceEntry.X, sliceEntry.Y);
                                var sliceSize = new Vector2(sliceEntry.Width, sliceEntry.Height);
                                var entryRect = new Rectangle(slicePosition, sliceSize);
                                var entryPivot = new Vector2(sliceEntry.PivotX / sliceEntry.Width, sliceEntry.PivotY / sliceEntry.Height);
                                var border = new Vector4();
                                if (!slice.Flags.HasFlag(SliceFlags.HasPivot)) {
                                    entryPivot = options.SpritePivot;
                                }
                                if (slice.Flags.HasFlag(SliceFlags.IsNinePatch)) {
                                    var left = sliceEntry.CenterX;
                                    var top = sliceEntry.CenterY;
                                    var right = sliceEntry.Width - (left + sliceEntry.CenterWidth);
                                    var bottom = sliceEntry.Height - (top + sliceEntry.CenterHeight);
                                    border = new Vector4(left, top, right, bottom);
                                }

                                sliceList.Add(new SliceInfo() {
                                    Position = slicePosition,
                                    Pivot = entryPivot,
                                    Size = sliceSize,
                                    Border = border,
                                    Name = $"{file.Name}_{slice.SliceName}_{sprites[index].Name}",
                                    FrameIndex = index,
                                });
                            }
                        }
                    }
                }
                var sprite = new Sprite {
                    TextureName = file.Name,
                    Rectangle = spriteRect,
                    Pivot = options.SpritePivot,
                    Border = Vector4.Zero,
                    FrameIndex = index,
                    Name = $"{file.Name}_{sprites[index].Name}"
                };
                importSprites.Add(sprite);

                index++;
                if (index >= sprites.Length)
                    break;
            }
            if (index >= sprites.Length)
                break;
        }

        // TODO: is there any reason to process these outside the main loop?
        if (options.EnableSliceImport) {
            for (var i = 0; i < sprites.Length; i++) {
                var cellX = (i * spriteSize.X) % width;
                var cellY = ((rows - 1) - ((i * spriteSize.X) / width)) * spriteSize.Y;
                foreach (var slice in sliceList) {
                    if (slice.FrameIndex > i) continue;
                    var slicePosition = slice.Position;
                    var sliceSize = slice.Size;
                    var sliceOffset = new Vector2(slicePosition.X, spriteSize.Y - slicePosition.Y - sliceSize.Y);
                    var sprite = new Sprite {
                        Rectangle = new Rectangle(new Vector2(cellX, cellY) + sliceOffset, sliceSize),
                        Pivot = slice.Pivot,
                        Border = slice.Border,
                        Name = $"{file.Name}_{slice.Name}_{sprites[i].Name}",
                        FrameIndex = slice.FrameIndex,
                    };
                    importSprites.Add(sprite);
                }
            }
        }
        return (atlas, importSprites);
    }

    public PixelBucket FlipTexture(PixelBucket input, bool horizontal = false, bool vertical = false) {
        PixelBucket flipped = new(input.Width, input.Height);
        AsepriteColor[] flipped_data = new AsepriteColor[input.Pixels.Length];

        if (!vertical && !horizontal) {
            flipped_data = input.Pixels;
        } else {
            for (int x = 0; x < input.Width; x++) {
                for (int y = 0; y < input.Height; y++) {
                    int index = 0;
                    if (horizontal && vertical)
                        index = input.Width - 1 - x + (input.Height - 1 - y) * input.Width;
                    else if (horizontal && !vertical)
                        index = input.Width - 1 - x + y * input.Width;
                    else if (!horizontal && vertical)
                        index = x + (input.Height - 1 - y) * input.Width;
                    else if (!horizontal && !vertical)
                        index = x + y * input.Width;

                    flipped_data[x + y * input.Width] = flipped.Pixels[index];
                }
            }
        }

        flipped.SetPixels(0, 0, input.Width, input.Height, flipped_data);

        return flipped;
    }

    private static bool TryGeneratePalette(PixelBucket sourceTexture, int paletteWidth, int paletteHeight, out PixelBucket palette) {
        int maxIndex = paletteHeight * paletteWidth - 1;
        palette = new PixelBucket(paletteWidth, paletteHeight);
        var paletteSwatch = new List<Color>(paletteHeight * paletteWidth);
        int height = sourceTexture.Height;
        int width = sourceTexture.Width;
        int idx = 0;

        // read the source texture top to bottom, left to right
        for (int y = height - 1; y >= 0; y--) {
            for (int x = 0; x < width; x++) {
                if (idx > maxIndex) {
                    return false;
                }

                var px = sourceTexture.GetPixel(x, y);
                if (px.a == 0.0f || paletteSwatch.Contains(px.ToColor())) continue;
                paletteSwatch.Add(px.ToColor());
                idx++;
            }
        }

        int paletteIdx = 0;
        foreach (var c in paletteSwatch) {
            var pos = GetPalettePosition(paletteIdx, paletteWidth, paletteHeight);
            palette.SetPixel(pos.x, pos.y, c.ToAseprite());
            paletteIdx++;
        }

        return true;
    }

    private static bool TryGeneratePaletteMap(PixelBucket sourceTexture, PixelBucket paletteTexture, out PixelBucket map, int paletteColorStep = 8) {
        int maxIndex = (paletteTexture.Width * paletteTexture.Height) - 1;
        map = new PixelBucket(sourceTexture.Width, sourceTexture.Height);
        var palette = FlattenPalette(paletteTexture);
        int height = sourceTexture.Height;
        int width = sourceTexture.Width;
        int idx = 0;

        // read the source texture top to bottom, left to right
        // i don't think the ordering really matters at this point
        // since the lookups are all tied to the palette
        for (int y = height - 1; y >= 0; y--) {
            for (int x = 0; x < width; x++) {
                if (idx > maxIndex) {
                    return false;
                }
                var px = sourceTexture.GetPixel(x, y);
                int paletteIndex = palette.IndexOf(px.ToColor());

                if (px.a == 0.0f || paletteIndex == -1) {
                    map.SetPixel(x, y, Color.White.ToAseprite());
                    if (px.a > 0.0f && paletteIndex == -1) {
                        var c = px;
                    }
                    continue;
                }

                var pos = GetPalettePosition(paletteIndex, paletteTexture.Width, paletteTexture.Height);
                var color = GetMapColor(pos, paletteColorStep);

                // Debug.Log($"INFO: [CreatePaletteMap] palette index: {paletteIndex}, palette color: {px.ToLogString()}, map color: {color.ToLogString()}, palette position: ({pos.x}, {pos.y})");

                map.SetPixel(x, y, color.ToAseprite());
            }
        }
        return true;
    }

    private static List<Color> FlattenPalette(PixelBucket paletteTexture) {
        var palette = new List<Color>(paletteTexture.Width * paletteTexture.Height);
        int height = paletteTexture.Height;
        int width = paletteTexture.Width;

        // read the palette from bottom to top, left to right
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                var px = paletteTexture.GetPixel(x, y);
                palette.Add(px.ToColor());
            }
        }

        return palette;
    }

    private static Color GetMapColor((int x, int y) position, int paletteColorStep = 8, bool applyGamma = false, float gammaValue = 2.2f) {
        float r = (position.x * paletteColorStep) / 255f;
        float g = (position.y * paletteColorStep) / 255f;

        if (applyGamma) {
            r = Maths.Pow(r, 1 / gammaValue);
            g = Maths.Pow(g, 1 / gammaValue);
        }

        var c = new Color(r, g, 0f, 1f);
        return c;
    }

    private static (int x, int y) GetPalettePosition(int index, int paletteWidth, int paletteHeight) {
        int y = index / paletteWidth;
        int x = index % paletteWidth;

        return (x, y);
    }

    private static List<AsepriteAnimation> ImportAnimations(AsepriteFile file, List<Sprite> sprites) {
        var animations = file.GetAnimations();
        var animSettings = new List<AsepriteAnimationSettings>();
        var spriteAnimations = new List<AsepriteAnimation>();
        if (animations.Length <= 0)
            return spriteAnimations;
        int index = 0;
        foreach(var animation in animations) {
            // build out an internal animation object
            var asset = new AsepriteAnimation() {
                Name = animation.TagName
            };

            var importSettings = GetAnimationSettingFor(animSettings, animation);
            importSettings.About = GetAnimationAbout(animation);

            asset.Frames = new List<AsepriteAnimationFrame>();
            int length = animation.FrameTo - animation.FrameFrom + 1;
            int from = (animation.Animation != LoopAnimation.Reverse) ? animation.FrameFrom : animation.FrameTo;
            int step = (animation.Animation != LoopAnimation.Reverse) ? 1 : -1;
            int keyIndex = from;

            for (int i = 0; i < length; i++) {
                if (i >= length) {
                    keyIndex = from;
                }

                var frame = new AsepriteAnimationFrame() {
                    Sprite = sprites[keyIndex],
                    FrameDuration = file.Frames[keyIndex].FrameDuration / 1000f,
                };

                keyIndex += step;
                asset.Frames.Add(frame);
            }

            switch (animation.Animation) {
                case LoopAnimation.Forward:
                    asset.Method = AnimationType.Looped;
                    importSettings.IsLooped = true;
                    break;
                case LoopAnimation.PingPong:
                    asset.Method = AnimationType.PingPong;
                    importSettings.IsLooped = true;
                    break;
                default:
                    asset.Method = AnimationType.Single;
                    importSettings.IsLooped = false;
                    break;
            }

            if (animation.TagColor == Color.White.ToAseprite()) {
                asset.Method = AnimationType.Single;
                importSettings.IsLooped = false;
            }
            index++;
            spriteAnimations.Add(asset);
            // store it in the asefile descriptor
        }
        return spriteAnimations;
    }

    private static AsepriteAnimationSettings GetAnimationSettingFor(List<AsepriteAnimationSettings> animationSettings, FrameTag animation) {
        if (animationSettings == null)
            animationSettings = new List<AsepriteAnimationSettings>();

        for (int i = 0; i < animationSettings.Count; i++) {
            if (animationSettings[i].AnimationName == animation.TagName)
                return animationSettings[i];
        }

        animationSettings.Add(new AsepriteAnimationSettings(animation.TagName));
        return animationSettings[animationSettings.Count - 1];
    }

    private static string GetAnimationAbout(FrameTag animation) {
        var sb = new StringBuilder();
        sb.AppendFormat("Animation Type:\t{0}", animation.Animation.ToString());
        sb.AppendLine();
        sb.AppendFormat("Animation:\tFrom: {0}; To: {1}", animation.FrameFrom, animation.FrameTo);

        return sb.ToString();
    }

    // step and replace all mask instances to clear
    private static AsepriteColor[] ReplaceMaskToTransparent(Color mask, AsepriteColor[] pallete) {
        for (int i = 0; i < pallete.Length; i++) {
            if (pallete[i] == mask.ToAseprite()) {
                pallete[i] = Color.Transparent.ToAseprite();
            }
        }

        return pallete;
    }

    private static int CalculateNextBaseTwoValue(int value) {
        var exponent = 0;
        var baseTwo = 0;

        while (baseTwo < value) {
            baseTwo = (int)System.Math.Pow(2, exponent);
            exponent++;
        }

        return baseTwo;
    }

}
