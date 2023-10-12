
using System.Runtime.InteropServices;
using SkiaSharp;
using Troublecat.Aseprite;
using Troublecat.Core.Graphics;

namespace Troublecat.Utilities;

public static class PngUtilities {
    public static bool WriteImage(string path, int width, int height, Color[] data) {
        using SKFileWStream stream = new(path);
        using SKBitmap bitmap = new(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
        int idx = 0;
        for (var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                idx = (y * width) + x;
                bitmap.SetPixel(x, y, data[idx]);
            }
        }
        return bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
    }

    public static bool WriteImage(string path, int width, int height, AsepriteColor[] data) {
        var colors = data.ToColor();
        return WriteImage(path, width, height, colors);
    }
}
