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


namespace GameBootstrap;

public class GameScene {
    protected IDataLoader _dataLoader;
    protected IAtlasTextureFactory _atlasTextureFactory;

    public Vector2 BaseScreenSize { get; set; }

    public void ConfigureServices(IServiceProvider services) {
        _dataLoader = services.GetRequiredService<IDataLoader>();
        _atlasTextureFactory = services.GetRequiredService<IAtlasTextureFactory>();
    }

    public virtual void LoadContent(IDataLoader dataLoader) {

    }

    public virtual void Update(XnaGameTime gameTime) {

    }

    public virtual void Draw(SpriteBatch spriteBatch) {

    }

    protected void DrawInternalTexture(SpriteBatch spriteBatch, InternalTexture texture, Vector2 pos, Vector2 scale, Rectangle clip, Color color) {
        if (texture.Coordinates != null && texture.Coordinates.HasValue) {
            var coords = (AtlasCoordinates)texture.Coordinates;
            DrawAtlas(spriteBatch, coords, pos, scale, clip, color);
        } else {
            var xnaTexture = _dataLoader.GetAsset<Texture2D>(texture.Texture2d!);
            DrawTexture(spriteBatch, xnaTexture, pos, scale, clip, color);
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

    protected void DrawInternalTexture(SpriteBatch spriteBatch, InternalTexture texture, Vector2 pos, Vector2 scale, Color color) {
        var xnaTexture = _dataLoader.GetAsset<Texture2D>(texture.Texture2d!);
        DrawTexture(spriteBatch, xnaTexture, pos, scale, new Rectangle(System.Numerics.Vector2.One * 0.5f, new Vector2(xnaTexture.Width, xnaTexture.Height)), color);
    }

    protected void DrawTexture(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Vector2 scale, Rectangle clip, Color color) {
        spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Vector2((int)position.X, (int)position.Y), clip, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}
