using System.Collections.Immutable;
using Troublecat.Core.Geometry;

namespace Troublecat.Core.Assets.Fonts;

public class PixelFontCharacter {
    public int Character;
    public Rectangle Glyph = Rectangle.Empty;
    public int XOffset;
    public int YOffset;
    public int XAdvance;
    public int Page;

    public ImmutableDictionary<int, int> Kerning = ImmutableDictionary<int, int>.Empty;

    public PixelFontCharacter() { }

    public PixelFontCharacter(int character, Rectangle rectangle, int xOffset, int yOffset, int xAdvance)
    {
        Character = character;
        Glyph = new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        XOffset = xOffset;
        YOffset = yOffset;
        XAdvance = xAdvance;
    }
}
