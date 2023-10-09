using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Graphics;
using Troublecat;
using Troublecat.Core.Assets.Fonts;
using Troublecat.Core.Graphics;
using Troublecat.Utilities;
using Troublecat.Core.Geometry;
using Microsoft.Extensions.DependencyInjection;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaRect = Microsoft.Xna.Framework.Rectangle;

namespace GameBootstrap;

public class BootstrappedGame : TroublecatGame {
    private PixelFont _font;
    private IAtlasTextureFactory _atlasTextureFactory;

    public BootstrappedGame(ILogger<BootstrappedGame> logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
    {
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void LoadContent() {
        base.LoadContent();
        _atlasTextureFactory ??= ServiceProvider.GetRequiredService<IAtlasTextureFactory>();
    }

    protected override void OnAfterAllContentLoaded() {
        _font = Loader.GetAsset<PixelFont>("fonts/ChevyRay-MagicBook");
    }

    protected override void DrawSprites(SpriteBatch batch) {
        base.DrawSprites(batch);
        DrawFont("Hi.", _font, batch, (BaseScreenSize/2).ToNumerics(), Vector2.One * ScreenScaling.Length(), Color.White, 2, shadowColor: Color.Black, strokeColor: Color.Orange);
    }

    public void DrawFont(string text, PixelFont font, SpriteBatch spriteBatch, Vector2 position, Vector2 scale,
        Color color, int visibleCharacters, float sort = 0, Color? strokeColor = null, Color? shadowColor = null, bool debugBox = false, Vector2? justify = null, Dictionary<int, Color?>? colors = null) {
        if (string.IsNullOrEmpty(text)) {
            return;
        }

        justify ??= Vector2.Zero;
        var justification = justify ?? Vector2.Zero;

        position = position.Floor();

        Vector2 offset = Vector2.Zero;
        Vector2 justified = new(font.FontSize!.WidthToNextLine(text, 0) * justification.X, font.FontSize!.HeightOf(text) * justification.Y);

        Color currentColor = color;

        // Index color, which will track the characters without a new line.
        int indexColor = 0;
        int lineCount = 1;

        // Keep track of the (actual) width of the line.
        float currentWidth = 0;
        float maxLineWidth = 0;

        // Finally, draw each character
        for (int i = 0; i < text.Length; i++, indexColor++) {
            var character = text[i];

            maxLineWidth = MathF.Max(maxLineWidth, currentWidth);
            if (character == '\n') {
                currentWidth = 0;

                lineCount++;
                offset.X = 0;
                offset.Y += font.FontSize.LineHeight * scale.Y + 1;
                if (justification.X != 0)
                    justified.X = font.FontSize.WidthToNextLine(text, i + 1) * justification.X;

                indexColor--;

                continue;
            }

            if (visibleCharacters >= 0 && i > visibleCharacters)
                break;

            if (font.FontSize.Characters.TryGetValue(character, out var c)) {
                Point pos = (position + (offset + new Vector2(c.XOffset, c.YOffset + font.FontSize.BaseLine + 1) * scale - justified)).Floor();


                var texture = font.FontSize.Textures[c.Page];
                //// draw stroke
                if (strokeColor.HasValue) {

                    if (shadowColor.HasValue) {
                        DrawInternalTexture(spriteBatch, texture, pos + new Point(-1, 2) * scale, scale, c.Glyph, shadowColor.Value);
                        DrawInternalTexture(spriteBatch, texture, pos + new Point(0, 2) * scale, scale, c.Glyph, shadowColor.Value);
                        DrawInternalTexture(spriteBatch, texture, pos + new Point(1, 2) * scale, scale, c.Glyph, shadowColor.Value);
                    }

                    DrawInternalTexture(spriteBatch, texture, pos + new Point(-1, -1) * scale, scale, c.Glyph, strokeColor.Value);
                    DrawInternalTexture(spriteBatch, texture, pos + new Point(0, -1) * scale, scale, c.Glyph, strokeColor.Value);
                    DrawInternalTexture(spriteBatch, texture, pos + new Point(1, -1) * scale, scale, c.Glyph, strokeColor.Value);
                    DrawInternalTexture(spriteBatch, texture, pos + new Point(-1, 0) * scale, scale, c.Glyph, strokeColor.Value);
                    DrawInternalTexture(spriteBatch, texture, pos + new Point(1, 0) * scale, scale, c.Glyph, strokeColor.Value);
                    DrawInternalTexture(spriteBatch, texture, pos + new Point(-1, 1) * scale, scale, c.Glyph, strokeColor.Value);
                    DrawInternalTexture(spriteBatch, texture, pos + new Point(0, 1) * scale, scale, c.Glyph, strokeColor.Value);
                    DrawInternalTexture(spriteBatch, texture, pos + new Point(1, 1) * scale, scale, c.Glyph, strokeColor.Value);
                } else if (shadowColor.HasValue) {
                    // Use 0.001f as the sort so draw the shadow under the font.
                    DrawInternalTexture(spriteBatch, texture, pos + new Point(0, 1), Vector2.One * scale, c.Glyph, shadowColor.Value);
                }

                if (colors is not null && colors.TryGetValue(indexColor, out Color? targetColorForText)) {
                    currentColor = targetColorForText * color.A ?? color;
                }

                // draw normal character
                DrawInternalTexture(spriteBatch, texture, pos, scale, c.Glyph, currentColor);


                offset.X += c.XAdvance * scale.X;
                currentWidth += c.XAdvance * scale.X;

                if (i < text.Length - 1 && c.Kerning.TryGetValue(text[i + 1], out int kerning))
                    offset.X += kerning * scale.X;
            }
        }
        //maxLineWidth = MathF.Max(maxLineWidth, currentWidth);

        // Point size = new Point(maxLineWidth, (font.FontSize.LineHeight + 1) * lineCount);
        // if (debugBox) {
        //     DrawRectangleOutline(spriteBatch, new Rectangle(position - size * justify, size), Color.White, 1, 0);
        // }
    }

    private void DrawInternalTexture(SpriteBatch spriteBatch, InternalTexture texture, Vector2 pos, Vector2 scale, Rectangle clip, Color color) {
        if (texture.Coordinates != null && texture.Coordinates.HasValue) {
            var coords = (AtlasCoordinates)texture.Coordinates;
            DrawAtlas(spriteBatch, coords, pos, scale, clip, color);
        } else {
            var xnaTexture = Loader.GetAsset<Texture2D>(texture.Texture2d!);
            DrawTexture(spriteBatch, xnaTexture, pos, scale, clip, color);
        }
    }

    private void DrawAtlas(SpriteBatch spriteBatch, AtlasCoordinates coords, Vector2 position, Vector2 scale, Rectangle clip, Color color) {
        var atlas = _atlasTextureFactory.GetAtlas(coords.AtlasId);
        var texture = atlas.Textures[coords.AtlasIndex];

        if (clip.IsEmpty) {
            spriteBatch.Draw(texture, position + new Vector2(0, 0).Rotate(0), coords.SourceRectangle, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
        } else {
            var intersection = Rectangle.GetIntersection(clip, coords.TrimArea);
            var adjustPosition = new Vector2(intersection.X - clip.X, intersection.Y - clip.Y);
            spriteBatch.Draw(texture, position + adjustPosition, new Rectangle(
                    coords.SourceRectangle.X - coords.TrimArea.X + intersection.X,
                    coords.SourceRectangle.Y - coords.TrimArea.Y + intersection.Y,
                    intersection.Width,
                    intersection.Height), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
    }

    private void DrawTexture(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Vector2 scale, Rectangle clip, Color color) {
        spriteBatch.Draw(texture, position + new Vector2(0, 0).Rotate(0), clip, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
    }
}
