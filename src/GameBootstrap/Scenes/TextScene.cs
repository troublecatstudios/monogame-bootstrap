
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Numerics;
using Troublecat;
using Troublecat.Core.Assets.Fonts;
using Troublecat.Core.Geometry;
using Troublecat.Data;
using Troublecat.Math;
using Troublecat.Utilities;

using XnaGameTime = Microsoft.Xna.Framework.GameTime;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaColor = Microsoft.Xna.Framework.Color;
using Troublecat.Core.Graphics;

namespace GameBootstrap.Scenes;

public class TextScene : GameScene {
    private PixelFont _font;
    private Vector2[] _characterPositions;
    private float _shakeTime;
    private int _randomIndex;
    private float _effectIntensity = 0.085f;
    private float _effectPerScroll = 0.05f;
    private float _fps = Constants.FPS_24;

    private static string _exampleText = "Welcome.";
    private int _lastScrollValue;
    private bool _lastEPressed;
    private bool _showStroke;
    private bool _showShadow;

    private List<Point> _strokePoints = new() {
        new Point(-1, -1),
        new Point(0, -1),
        new Point(1, -1),
        new Point(-1, 0),
        new Point(1, 0),
        new Point(-1, 1),
        new Point(0, 1),
        new Point(1, 1),
    };

    private List<Point> _shadowPoints = new() {
        new Point(-1, 2),
        new Point(0, 2),
        new Point(1, 2),
    };

    public override void LoadContent(IDataLoader dataLoader) {
        _characterPositions = new Vector2[_exampleText.Length];
        _font = dataLoader.GetAsset<PixelFont>("fonts/ChevyRay-MagicBook");
    }

    public override void Update(XnaGameTime gameTime) {
        base.Update(gameTime);
        var totalTime = (float)gameTime.TotalGameTime.TotalSeconds;
        _shakeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

        var mouseState = Mouse.GetState();
        if (mouseState.ScrollWheelValue != 0) {
            var delta = mouseState.ScrollWheelValue - _lastScrollValue;
            var sign = MathF.Sign(delta);
            var scrollScaled = delta/120f;
            var scrollClamped = Maths.Clamp(scrollScaled, 0.1f, 8f) * sign;
            _effectIntensity += scrollClamped * _effectPerScroll;
            _effectIntensity = Maths.Clamp(_effectIntensity, 0.085f, 500f);
            _lastScrollValue = mouseState.ScrollWheelValue;
        }

        var kbState = Keyboard.GetState();
        if (kbState.IsKeyDown(Keys.E) && !_lastEPressed) {
            if (_fps == Constants.FPS_24) _fps = Constants.FPS_12;
            else _fps = Constants.FPS_24;
        }
        _lastEPressed = kbState.IsKeyDown(Keys.E);

        if (_shakeTime > 1f) {
            _shakeTime = 0f;
            var newRnd = Randoms.InRange(0, Randoms.FakeRandomsCount);
            //Avoids repeating the same index twice
            if (_randomIndex == newRnd) {
                newRnd++;
                if (newRnd >= Randoms.FakeRandomsCount)
                    newRnd = 0;
            }
            _randomIndex = newRnd;

            for(var i = 0; i < _exampleText.Length; i++) {
                _characterPositions[i] = DoShake(i, _randomIndex, 1f, _effectIntensity);
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch) {
        DrawFont(_exampleText, _font, spriteBatch, (BaseScreenSize/2), Vector2.One, Color.White, shadowOffset: new Vector2(5, 1), shadowColor: Color.Black, strokeColor: Color.Orange, positions: _characterPositions);
    }

    public void DrawFont(string text,
                         PixelFont font,
                         SpriteBatch spriteBatch,
                         Vector2 position,
                         Vector2 scale,
                         Color color,
                         float sort = 0,
                         Color? strokeColor = null,
                         Color? shadowColor = null,
                         bool debugBox = false,
                         Vector2? justify = null,
                         Dictionary<int, Color?>? colors = null,
                         Vector2[]? positions = null,
                         Vector2? shadowOffset = null) {
        DrawFont(text, font, spriteBatch, position, scale, color, text.Length, sort, strokeColor, shadowColor, debugBox, justify, colors, positions, shadowOffset);
    }

    public void DrawFont(string text,
                         PixelFont font,
                         SpriteBatch spriteBatch,
                         Vector2 position,
                         Vector2 scale,
                         Color color,
                         int visibleCharacters,
                         float sort = 0,
                         Color? strokeColor = null,
                         Color? shadowColor = null,
                         bool debugBox = false,
                         Vector2? justify = null,
                         Dictionary<int, Color?>? colors = null,
                         Vector2[]? positions = null,
                         Vector2? shadowOffset = null) {
        if (string.IsNullOrEmpty(text)) {
            return;
        }

        var justification = justify ?? new Vector2(0.5f, 0.5f);

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

                if (positions is not null) {
                    pos.X += Maths.FloorToInt(positions[indexColor].X);
                    pos.Y += Maths.FloorToInt(positions[indexColor].Y);
                }

                //// draw stroke
                if (strokeColor.HasValue) {

                    if (shadowColor.HasValue) {
                        for(var ii = 0; ii < _shadowPoints.Count; ii++) {
                            var shadowPoint = (shadowOffset == null ? Vector2.One : shadowOffset.Value);
                            DrawInternalTexture(spriteBatch, texture, pos + _shadowPoints[ii] + shadowPoint * scale, scale, c.Glyph, shadowColor.Value);
                        }
                    }

                    for(var ii = 0; ii < _strokePoints.Count; ii++) {
                        DrawInternalTexture(spriteBatch, texture, pos + _strokePoints[ii] * scale, scale, c.Glyph, strokeColor.Value);
                    }
                } else if (shadowColor.HasValue) {
                    // Use 0.001f as the sort so draw the shadow under the font.
                    var shadowOffsetFinal = (shadowOffset == null ? Vector2.One : shadowOffset.Value);
                    DrawInternalTexture(spriteBatch, texture, pos + new Point(0, 1) + shadowOffsetFinal * scale, Vector2.One * scale, c.Glyph, shadowColor.Value);
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

    private Vector2 DoShake(int characterIndex, int randomIndex, float shakeStrength, float effectIntensity) {
        var index = Maths.RoundToInt((characterIndex + randomIndex) % (Randoms.FakeRandomsCount - 1));
        var v3 = Randoms.FakeRandoms[index] * shakeStrength * effectIntensity;
        return new Vector2(v3.X, v3.Y);
    }
}
