using System.Text.RegularExpressions;
using Troublecat.Assets;
using Troublecat.Core.Graphics;

namespace Troublecat.Core.Assets.Fonts;

public class PixelFont {
    private readonly PixelFontSize? _pixelFontSize;

    public static string Escape(string text) => Regex.Replace(text, "<c=([^>]+)>|</c>", "");

    public Guid Index;

    public int LineHeight => _pixelFontSize?.LineHeight ?? 0;

    public PixelFontSize? FontSize => _pixelFontSize;

    public PixelFont(FontAsset asset) {
        // get texture
        var textures = new List<InternalTexture>();
        textures.Add(new InternalTexture($"fonts/{Path.GetFileNameWithoutExtension(asset.TexturePath)}"));

        Index = asset.Guid;

        // create font size
        var fontSize = new PixelFontSize() {
            Textures = textures,
            Characters = new Dictionary<int, PixelFontCharacter>(),
            LineHeight = asset.LineHeight,
            BaseLine = asset.Baseline
        };

        // get characters
        foreach (var character in asset.Characters) {
            fontSize.Characters.Add(character.Key, character.Value);
        }

        // get kerning
        foreach (var kerning in asset.Kernings) {
            var from = kerning.First;
            var to = kerning.Second;
            var push = kerning.Amount;

            if (fontSize.Characters.TryGetValue(from, out var c))
                c.Kerning.Add(to, push);
        }

        // add font size
        _pixelFontSize = fontSize;

        //Sizes.Add(fontSize);
        //Sizes.Sort((a, b) => { return Math.Sign(a.Size - b.Size); });
    }

    public float GetLineWidth(ReadOnlySpan<char> text) {
        if (_pixelFontSize is null) {
            throw new Exception("Pixel font size was not initialized.");
        }

        //var font = Get(size);
        float width = _pixelFontSize.WidthToNextLine(text, 0);
        return width;
    }
}
