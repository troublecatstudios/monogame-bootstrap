using Troublecat.Aseprite;
using Troublecat.Core.Graphics;

namespace Troublecat.Utilities;

public static class ColorExtensions {
    public static AsepriteColor ToAseprite(this Color color) => new AsepriteColor(color.R, color.G, color.B, color.A);
    public static AsepriteColor[] ToAseprite(this Color[] colors) => colors.Select(c => c.ToAseprite()).ToArray();
    public static Color ToColor(this AsepriteColor color) => new(color.R, color.G, color.B, color.A);
    public static Color[] ToColor(this AsepriteColor[] colors) => colors.Select(c => c.ToColor()).ToArray();
}
