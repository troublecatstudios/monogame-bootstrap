using System.Collections.Immutable;
using Troublecat.Assets.Fonts;

namespace Troublecat.Assets;

public class FontAsset
{
    public string Name = string.Empty;
    public string TexturePath = string.Empty;
    public int LineHeight = 0;
    public float Baseline;
    public readonly ImmutableDictionary<int, PixelFontCharacter> Characters = ImmutableDictionary<int, PixelFontCharacter>.Empty;

    public readonly ImmutableArray<Kerning> Kernings = ImmutableArray<Kerning>.Empty;

    public FontAsset(Dictionary<int, PixelFontCharacter> characters, ImmutableArray<Kerning> kernings, int size, string texturePath, float baseline)
    {
        Name = Path.GetFileNameWithoutExtension(texturePath);
        LineHeight = size;
        Baseline = baseline;
        TexturePath = Name + ".png";

        Characters = characters.ToImmutableDictionary();
        Kernings = kernings;
    }

    public FontAsset()
    {
    }
}
