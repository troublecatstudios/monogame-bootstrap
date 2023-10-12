using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Troublecat.Aseprite.Abstractions;
using Troublecat.Aseprite.Chunks;
using Troublecat.Aseprite.Utils;


namespace Troublecat.Aseprite;

/// <summary>
/// Represents a *.ase or *.aseprite file
/// </summary>
/// <remarks>
/// See file specs here: https://github.com/aseprite/aseprite/blob/master/docs/ase-file-specs.md
/// </remarks>
public class AsepriteFile {
    public static AsepriteFile FromFile(string assetPath) {
        FileStream fileStream = new(assetPath, FileMode.Open, FileAccess.Read);
        AsepriteFile aseFile = new(fileStream, Path.GetFileName(assetPath));
        fileStream.Close();

        return aseFile;
    }

    private readonly Dictionary<Type, AsepriteFileChunk> _chunkCache = new();
    private readonly Texture2DBlender _blender;

    public string Name { get; set; }

    public Header Header { get; private set; }
    public List<Frame> Frames { get; private set; }
    public List<LayerChunk> Layers => GetChunks<LayerChunk>();

    public AsepriteFile(Stream stream, string name, Texture2DBlender? blender = null) {
        _blender = blender ?? new Texture2DBlender();
        BinaryReader reader = new(stream);
        byte[] header = reader.ReadBytes(128);

        Name = name;

        Header = new Header(header);
        Frames = new();

        while (reader.BaseStream.Position < reader.BaseStream.Length) {
            Frames.Add(new Frame(this, reader));
        }
    }

    public List<T> GetChunks<T>() where T : AsepriteFileChunk {
        List<T> chunks = new();

        for (int i = 0; i < Frames.Count; i++) {
            List<T> cs = Frames[i].GetChunks<T>();

            chunks.AddRange(cs);
        }

        return chunks;
    }

    public T GetChunk<T>() where T : AsepriteFileChunk {
        if (!_chunkCache.ContainsKey(typeof(T))) {
            for (int i = 0; i < Frames.Count; i++) {
                List<T> cs = Frames[i].GetChunks<T>();

                if (cs.Count > 0) {
                    _chunkCache.Add(typeof(T), cs[0]);
                    break;
                }
            }
        }

        return (T)_chunkCache[typeof(T)];
    }

    public PixelBucket[] GetFrames() {
        var frames = new List<PixelBucket>();

        for (int i = 0; i < Frames.Count; i++) {
            frames.Add(GetFramePixels(i));
        }

        return frames.ToArray();
    }


    public PixelBucket[] GetLayersAsFrames() {
        var frames = new List<PixelBucket>();
        var layers = GetChunks<LayerChunk>();

        for (int i = 0; i < layers.Count; i++) {
            var layerFrames = GetLayerPixels(i, layers[i]);

            if (layerFrames.Count > 0) {
                frames.AddRange(layerFrames);
            }
        }

        return frames.ToArray();
    }

    private LayerChunk? GetParentLayer(LayerChunk layer) {
        if (layer.LayerChildLevel == 0)
            return null;

        var layers = GetChunks<LayerChunk>();
        var index = layers.IndexOf(layer);

        if (index < 0)
            return null;

        for (int i = index - 1; i > 0; i--) {
            if (layers[i].LayerChildLevel == layer.LayerChildLevel - 1)
                return layers[i];
        }

        return null;
    }

    public List<PixelBucket> GetLayerPixels(int layerIndex, LayerChunk layer, bool forceVisible = false) {
        var textures = new List<PixelBucket>();

        for (int frameIndex = 0; frameIndex < Frames.Count; frameIndex++) {
            var frame = Frames[frameIndex];
            var cels = frame.GetChunks<CelChunk>();

            for (int i = 0; i < cels.Count; i++) {
                if (cels[i].LayerIndex != layerIndex)
                    continue;

                var blendMode = layer.BlendMode;
                var opacity = InternalMath.Min(layer.Opacity / 255f, cels[i].Opacity / 255f);

                var visibility = forceVisible || layer.Visible;

                var parent = GetParentLayer(layer);
                while (parent != null) {
                    visibility &= parent.Visible;
                    if (visibility == false)
                        break;

                    parent = GetParentLayer(parent);
                }

                if (visibility == false || layer.LayerType == LayerType.Group)
                    continue;

                var tex = GetCelPixels(cels[i]);
                tex.Name = $"{layer.LayerName}_{i}";
                textures.Add(tex);
            }
        }

        return textures;
    }

    public PixelBucket GetFramePixels(int index) {
        var frame = Frames[index];
        var texture = new PixelBucket(Header.Width, Header.Height);
        var layers = GetChunks<LayerChunk>();
        var cels = frame.GetChunks<CelChunk>();

        cels.Sort((ca, cb) => ca.LayerIndex.CompareTo(cb.LayerIndex));

        for (int i = 0; i < cels.Count; i++) {
            var layer = layers[cels[i].LayerIndex];
            if (layer.LayerName.StartsWith("@")) //ignore metadata layer
                continue;

            var blendMode = layer.BlendMode;
            float opacity = InternalMath.Min(layer.Opacity / 255f, cels[i].Opacity / 255f);

            bool visibility = layer.Visible;


            var parent = GetParentLayer(layer);
            while (parent != null) {
                visibility &= parent.Visible;
                if (visibility == false)
                    break;

                parent = GetParentLayer(parent);
            }

            if (visibility == false || layer.LayerType == LayerType.Group)
                continue;

            var celTex = GetCelPixels(cels[i]);

            switch (blendMode) {
                case LayerBlendMode.Normal: texture = Texture2DBlender.Normal(texture, celTex); break;
                case LayerBlendMode.Multiply: texture = Texture2DBlender.Multiply(texture, celTex, opacity); break;
                case LayerBlendMode.Screen: texture = Texture2DBlender.Screen(texture, celTex); break;
                case LayerBlendMode.Overlay: texture = Texture2DBlender.Overlay(texture, celTex); break;
                case LayerBlendMode.Darken: texture = Texture2DBlender.Darken(texture, celTex); break;
                case LayerBlendMode.Lighten: texture = Texture2DBlender.Lighten(texture, celTex); break;
                case LayerBlendMode.ColorDodge: texture = Texture2DBlender.ColorDodge(texture, celTex); break;
                case LayerBlendMode.ColorBurn: texture = Texture2DBlender.ColorBurn(texture, celTex); break;
                case LayerBlendMode.HardLight: texture = Texture2DBlender.HardLight(texture, celTex); break;
                case LayerBlendMode.SoftLight: texture = Texture2DBlender.SoftLight(texture, celTex); break;
                case LayerBlendMode.Difference: texture = Texture2DBlender.Difference(texture, celTex); break;
                case LayerBlendMode.Exclusion: texture = Texture2DBlender.Exclusion(texture, celTex); break;
                case LayerBlendMode.Hue: texture = Texture2DBlender.Hue(texture, celTex); break;
                case LayerBlendMode.Saturation: texture = Texture2DBlender.Saturation(texture, celTex); break;
                case LayerBlendMode.Color: texture = Texture2DBlender.Color(texture, celTex); break;
                case LayerBlendMode.Luminosity: texture = Texture2DBlender.Luminosity(texture, celTex); break;
                case LayerBlendMode.Addition: texture = Texture2DBlender.Addition(texture, celTex); break;
                case LayerBlendMode.Subtract: texture = Texture2DBlender.Subtract(texture, celTex); break;
                case LayerBlendMode.Divide: texture = Texture2DBlender.Divide(texture, celTex); break;
            }
        }

        texture.Name = $"{index}";

        return texture;
    }

    private PixelBucket GetCelPixels(CelChunk cel) {
        int canvasWidth = Header.Width;
        int canvasHeight = Header.Height;

        var texture = new PixelBucket(canvasWidth, canvasHeight);

        int pixelIndex = 0;
        int celXEnd = cel.Width + cel.X;
        int celYEnd = cel.Height + cel.Y;


        for (int y = cel.Y; y < celYEnd; y++) {
            if (y < 0 || y >= canvasHeight) {
                pixelIndex += cel.Width;
                continue;
            }

            for (int x = cel.X; x < celXEnd; x++) {
                if (x >= 0 && x < canvasWidth) {
                    var color = cel.RawPixelData[pixelIndex].GetColor();
                    texture.SetPixel(x, y, color);
                }

                ++pixelIndex;
            }
        }

        return texture;
    }

    public FrameTag[] GetAnimations() {
        var tagChunks = GetChunks<FrameTagsChunk>();

        var animations = new List<FrameTag>();

        foreach (FrameTagsChunk tagChunk in tagChunks) {
            foreach (FrameTag tag in tagChunk.Tags) {
                animations.Add(tag);
            }
        }

        return animations.ToArray();
    }

    public Metadata[] GetMetaData(Vector2 spritePivot, int pixelsPerUnit = 1) {
        var metadatas = new Dictionary<int, Metadata>();

        for (int index = 0; index < Frames.Count; index++) {
            var layers = GetChunks<LayerChunk>();
            var cels = Frames[index].GetChunks<CelChunk>();

            cels.Sort((ca, cb) => ca.LayerIndex.CompareTo(cb.LayerIndex));

            for (int i = 0; i < cels.Count; i++) {
                int layerIndex = cels[i].LayerIndex;
                var layer = layers[layerIndex];
                if (!layer.LayerName.StartsWith(Metadata.MetadataCharPrefix)) //read only metadata layer
                    continue;

                if (!metadatas.ContainsKey(layerIndex))
                    metadatas[layerIndex] = new Metadata(layer, layerIndex);
                var metadata = metadatas[layerIndex];

                var cel = cels[i];
                var center = Vector2.Zero;
                int pixelCount = 0;

                for (int y = 0; y < cel.Height; ++y) {
                    for (int x = 0; x < cel.Width; ++x) {
                        int texX = cel.X + x;
                        int texY = -(cel.Y + y) + Header.Height - 1;
                        var col = cel.RawPixelData[x + y * cel.Width];
                        if (col.GetColor().a > 0.1f) {
                            center += new Vector2(texX, texY);
                            pixelCount++;
                        }
                    }
                }

                if (pixelCount > 0) {
                    center /= pixelCount;
                    var pivot = spritePivot.Scale(new(Header.Width, Header.Height));
                    var posWorld = (center - pivot) / pixelsPerUnit + Vector2.One * 0.5f / pixelsPerUnit; //center pos in middle of pixels

                    metadata.Transforms.Add(index, posWorld);
                }
            }
        }
        return metadatas.Values.ToArray();
    }

    public PixelBucket GetTexturePixels() {
        var frames = GetFrames();
        var atlas = new PixelBucket(Header.Width * frames.Length, Header.Height);
        var col = 0;
        var row = 0;

        foreach (var frame in frames) {
            var startX = col * Header.Width;
            var startY = atlas.Height - (row + 1) * Header.Height;
            var width = Header.Width;
            var height = Header.Height;

            int idx = 0;
            foreach (var px in frame.Pixels) {
                var x = idx % width;
                var y = idx / width;
                atlas.SetPixel(startX + x, startY + y, px);
                idx++;
            }
            col++;
        }

        return atlas;
    }
}
