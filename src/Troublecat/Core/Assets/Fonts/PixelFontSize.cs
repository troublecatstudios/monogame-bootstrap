using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Graphics;
using Troublecat.Core.Graphics;

namespace Troublecat.Core.Assets.Fonts;

public class PixelFontSize {
    public List<InternalTexture> Textures = new();
    public Dictionary<int, PixelFontCharacter> Characters = new();
    public int LineHeight;
    public float BaseLine;

    private readonly StringBuilder _temp = new StringBuilder();
    private record struct TextCacheData(string Text, int Width) { }

    public string AutoNewline(string text, int width)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        _temp.Clear();

        var words = Regex.Split(text, @"(\s)");
        var lineWidth = 0f;

        foreach (var word in words)
        {
            var wordWidth = Measure(word).X;
            if (wordWidth + lineWidth > width)
            {
                _temp.Append('\n');
                lineWidth = 0;

                if (word.Equals(" "))
                    continue;
            }

            // this word is longer than the max-width, split where ever we can
            if (wordWidth > width)
            {
                int i = 1, start = 0;
                for (; i < word.Length; i++)
                    if (i - start > 1 && Measure(word.Substring(start, i - start - 1)).X > width)
                    {
                        _temp.Append(word.Substring(start, i - start - 1));
                        _temp.Append('\n');
                        start = i - 1;
                    }


                var remaining = word.Substring(start, word.Length - start);
                _temp.Append(remaining);
                lineWidth += Measure(remaining).X;
            }
            // normal word, add it
            else
            {
                lineWidth += wordWidth;
                _temp.Append(word);
            }
        }

        return _temp.ToString();
    }

    public Vector2 Measure(string text)
    {
        if (string.IsNullOrEmpty(text))
            return Vector2.Zero;

        var size = new Vector2(0, LineHeight);
        var currentLineWidth = 0f;

        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                size.Y += LineHeight + 1;
                if (currentLineWidth > size.X)
                    size.X = currentLineWidth;
                currentLineWidth = 0f;
            }
            else
            {
                PixelFontCharacter? c = null;
                if (Characters.TryGetValue(text[i], out c))
                {
                    currentLineWidth += c.XAdvance;

                    int kerning;
                    if (i < text.Length - 1 && c.Kerning.TryGetValue(text[i + 1], out kerning))
                        currentLineWidth += kerning;
                }
            }
        }

        if (currentLineWidth > size.X)
            size.X = currentLineWidth;

        return size;
    }

    public float WidthToNextLine(ReadOnlySpan<char> text, int start, bool trimWhitespace = true)
    {
        if (text.IsEmpty)
        {
            return 0;
        }

        var currentLineWidth = 0f;

        int i, j;
        for (i = start, j = text.Length; i < j; i++)
        {
            if (text[i] == '\n')
                break;

            PixelFontCharacter? c = null;
            if (Characters.TryGetValue(text[i], out c))
            {
                currentLineWidth += c.XAdvance;
                int kerning;
                if (i < j - 1 && c.Kerning.TryGetValue(text[i + 1], out kerning))
                    currentLineWidth += kerning;
            }
        }

        // Trim ending whitepace
        if (trimWhitespace)
        {
            i--;
            if (i > 0 && text.Length > i && (text[i] == ' ' || text[i] == '\n'))
            {
                PixelFontCharacter? c = null;
                if (Characters.TryGetValue(text[i], out c))
                {
                    currentLineWidth -= c.XAdvance;
                    int kerning;
                    if (i < j - 1 && c.Kerning.TryGetValue(text[i + 1], out kerning))
                        currentLineWidth -= kerning;

                }
            }
        }
        return currentLineWidth;
    }

    public float HeightOf(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        int lines = 1;
        if (text.IndexOf('\n') >= 0)
            for (int i = 0; i < text.Length; i++)
                if (text[i] == '\n')
                    lines++;
        return lines * LineHeight;
    }

    private string WrapString(ReadOnlySpan<char> text, int maxWidth, float scale, ref int visibleCharacters)
    {
        Vector2 offset = Vector2.Zero;

        StringBuilder wrappedText = new StringBuilder();
        int cursor = 0;
        while (cursor < text.Length)
        {
            bool alreadyHasLineBreak = false;

            int nextSpaceIndex = text.Slice(cursor).IndexOf(' ');
            int nextLineBreak = text.Slice(cursor).IndexOf('\n');

            if (nextLineBreak >= 0 && nextLineBreak < nextSpaceIndex)
            {
                alreadyHasLineBreak = true;
                nextSpaceIndex = nextLineBreak + cursor;
            }
            else if (nextSpaceIndex >= 0)
            {
                nextSpaceIndex = nextSpaceIndex + cursor;
            }

            if (nextSpaceIndex == -1)
                nextSpaceIndex = text.Length - 1;

            ReadOnlySpan<char> word = text.Slice(cursor, nextSpaceIndex - cursor + 1);
            float wordWidth = WidthToNextLine(word, 0, false) * scale;
            bool overflow = offset.X + wordWidth > maxWidth;

            if (overflow)
            {
                // Has overflow, word is going down.
                wrappedText.Append('\n');
                visibleCharacters += 1;
                offset.X = wordWidth;
            }
            else
            {
                // Didn't break line
                offset.X += wordWidth;
            }

            if (alreadyHasLineBreak)
            {
                // Snap to zero.
                offset.X = 0;
            }

            // Make sure we also take the new line into consideration.
            if (visibleCharacters > cursor)
            {
                visibleCharacters += word.Length;
            }

            wrappedText.Append(word);

            cursor = nextSpaceIndex + 1;
        }

        return wrappedText.ToString();
    }
}
