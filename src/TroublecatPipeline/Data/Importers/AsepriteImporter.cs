
using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework.Graphics;
using Troublecat.Aseprite;
using Troublecat.Aseprite.Abstractions;
using Troublecat.Aseprite.Chunks;
using Troublecat.Core.Assets;
using Troublecat.Core.Diagnostics;
using Troublecat.Core.Geometry;
using Troublecat.Core.Graphics;
using Troublecat.Data.Importers.Aseprite;
using Troublecat.Data.Serialization;
using Troublecat.IO;
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

    public async Task ImportAsync(string resourcePath) {
        using (_logger.BeginScope(new PerfScope())) {
            if (!Paths.Exists(resourcePath)) {
                _logger.LogError($"Resource not found! Does '{resourcePath}' exist?!");
                return;
            }
            var packPath = Paths.GetPath(_configuration.ResourcesPackDirectoryAbsolute, "Sprites");
            var fileName = Path.GetFileNameWithoutExtension(resourcePath);

            var jsonFile = $"{fileName}.json";
            var jsonPackPath = Paths.GetPath(packPath, jsonFile);

            string GetTexturePackPath(string textureName) => Paths.GetPath(packPath!, $"{textureName}.png");

            AsepriteImportOptions options = new();
            _configuration.Sprites.TryGetValue(fileName, out options);
            _logger.LogInformation($"Importing {resourcePath}");

            // load the aseprite file from disk
            var file = AsepriteFile.FromFile(resourcePath);
            int frameCount = file.Header.Frames;
            var descriptor = new AsepriteDescriptor();

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

            // create palette swap texture as well

            // start saving the assets

            // write out the atlas
            descriptor.AtlasTexturePath = GetTexturePackPath($"{file.Name}_atlas");
            PngUtilities.WriteImage(descriptor.AtlasTexturePath, atlasPack.Atlas.Width, atlasPack.Atlas.Height, atlasPack.Atlas.Pixels);

            // write out each of the secondary textures
            foreach(var entry in secondaryTextures) {
                var secondaryPath = GetTexturePackPath($"{file.Name}_{entry.Key}");
                descriptor.SecondaryTexturePaths.Add(entry.Key, secondaryPath);
                PngUtilities.WriteImage(secondaryPath, entry.Value.Width, entry.Value.Height, entry.Value.Pixels);
            }

            descriptor.Entries = atlasPack.Entries;
            descriptor.FrameCount = frames.Length;
            descriptor.Metadata = metadata;

            string json = await _serializer.SerializeAsync(descriptor, false);
            await Files.SaveTextAsync(json, jsonPackPath);
        }
        return;
    }

    private (PixelBucket Atlas, List<AsepriteAtlasEntry> Entries) CreateAtlas(PixelBucket[] sprites, AsepriteFile file, AsepriteImportOptions options, bool baseTwo = true) {
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

    private (PixelBucket Atlas, List<AsepriteAtlasEntry> Entries) CreateAtlas(PixelBucket[] sprites, Vector2 spriteSize, AsepriteFile file, int cols, int rows, AsepriteImportOptions options, bool baseTwo = true) {
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

        var importEntries = new List<AsepriteAtlasEntry>();

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
                var entry = new AsepriteAtlasEntry {
                    Rectangle = spriteRect,
                    Pivot = options.SpritePivot,
                    Border = Vector4.Zero,
                    Name = $"{file.Name}_{sprites[index].Name}"
                };
                importEntries.Add(entry);

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
                    var entry = new AsepriteAtlasEntry {
                        Rectangle = new Rectangle(new Vector2(cellX, cellY) + sliceOffset, sliceSize),
                        Pivot = slice.Pivot,
                        Border = slice.Border,
                        Name = $"{file.Name}_{slice.Name}_{sprites[i].Name}"
                    };
                    importEntries.Add(entry);
                }
            }
        }
        return (atlas, importEntries);
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
