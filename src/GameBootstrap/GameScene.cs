using Microsoft.Xna.Framework.Graphics;
using Troublecat.Core.Graphics;
using Troublecat.Data;
using System.Numerics;
using Troublecat.Core.Geometry;
using Troublecat.Utilities;

using XnaGameTime = Microsoft.Xna.Framework.GameTime;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaColor = Microsoft.Xna.Framework.Color;
using Microsoft.Extensions.DependencyInjection;
using Troublecat.Math;
using Troublecat.Core.Assets.Fonts;
using Troublecat.Core;
using GameBootstrap.Components;
using Troublecat.Core.Assets.Sprites;


namespace GameBootstrap;

public class GameScene {
    protected IDataLoader _dataLoader;
    protected IAtlasTextureFactory _atlasTextureFactory;
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

    protected List<IComponent> Components { get; private set; } = new();

    public Vector2 BaseScreenSize { get; set; }

    public Vector2 CenterScreen => (BaseScreenSize/2);

    public void ConfigureServices(IServiceProvider services) {
        _dataLoader = services.GetRequiredService<IDataLoader>();
        _atlasTextureFactory = services.GetRequiredService<IAtlasTextureFactory>();
    }

    public T AddComponent<T>() where T : IComponent, new() {
        var component = new T();
        Components.Add(component);
        return component;
    }

    public virtual void LoadContent(IDataLoader dataLoader) {

    }

    public virtual void Update(Timing time) {
        UpdateComponents(time);
    }

    public virtual void Draw(SpriteBatch spriteBatch) {
        DrawComponents(spriteBatch);
    }

    protected virtual void UpdateComponents(Timing time) {
        foreach(var c in Components) {
            if (c is IUpdateableComponent updateable) {
                updateable.Update(time);
            }
        }
    }

    protected virtual void DrawComponents(SpriteBatch spriteBatch) {
        foreach(var c in Components) {
            if (c is IDrawableComponent drawable) {
                drawable.Draw(spriteBatch);
            }
        }
    }

    protected void DrawInternalTexture(SpriteBatch spriteBatch, InternalTexture texture, Vector2 pos, Vector2 scale, Rectangle clip, Color color, Vector2? origin = null) {
        if (texture.Coordinates != null && texture.Coordinates.HasValue) {
            var coords = (AtlasCoordinates)texture.Coordinates;
            DrawAtlas(spriteBatch, coords, pos, scale, clip, color);
        } else {
            var xnaTexture = _dataLoader.GetAsset<Texture2D>(texture.Texture2d!);
            DrawTexture(spriteBatch, xnaTexture, pos, scale, clip, color, origin ?? Vector2.Zero);
        }
    }

    protected void DrawAtlas(SpriteBatch spriteBatch, AtlasCoordinates coords, Vector2 position, Vector2 scale, Rectangle clip, Color color) {
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

    protected void DrawSprite(SpriteBatch spriteBatch, Sprite sprite, Vector2 position, Vector2 scale, Color color, float roation = 0f) {
        var xnaTexture = _dataLoader.GetAsset<Texture2D>(sprite.TextureName);
        spriteBatch.Draw(xnaTexture, position, sprite.Rectangle, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
    }

    protected void DrawInternalTexture(SpriteBatch spriteBatch, InternalTexture texture, Vector2 pos, Vector2 scale, Color color, float rotation = 0f) {
        var xnaTexture = _dataLoader.GetAsset<Texture2D>(texture.Texture2d!);
        var dimensions = new Vector2(xnaTexture.Width, xnaTexture.Height);
        var origin = Vector2.One/2 * dimensions;
        DrawTexture(spriteBatch, xnaTexture, pos, scale, new Rectangle(Vector2.One/2, dimensions), color, origin, rotation);
    }

    protected void DrawTexture(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Vector2 scale, Rectangle clip, Color color, Vector2 origin, float rotation = 0f) {
        spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Vector2((int)position.X, (int)position.Y), clip, color, rotation, origin, scale, SpriteEffects.None, 0f);
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
    }
}
