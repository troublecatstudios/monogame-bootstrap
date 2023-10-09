using Microsoft.Xna.Framework.Graphics;
using Troublecat.Core.Graphics;
using Troublecat.Data;

using XnaGameTime = Microsoft.Xna.Framework.GameTime;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaColor = Microsoft.Xna.Framework.Color;
using System.Numerics;

namespace GameBootstrap.Scenes;

public class FaceScene : GameScene {
    private InternalTexture _face;

    public override void LoadContent(IDataLoader dataLoader) {
        _face = dataLoader.GetAsset<InternalTexture>("sprites/test-face");
    }

    public override void Update(XnaGameTime gameTime) {
        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch) {
        DrawInternalTexture(spriteBatch, _face, new Vector2(BaseScreenSize.X/2 - 32, BaseScreenSize.Y/2 - 80), Vector2.One, Color.White);
    }
}
