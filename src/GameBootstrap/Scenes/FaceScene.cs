using Microsoft.Xna.Framework.Graphics;
using Troublecat.Core.Graphics;
using Troublecat.Data;

using XnaGameTime = Microsoft.Xna.Framework.GameTime;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaColor = Microsoft.Xna.Framework.Color;
using System.Numerics;
using Troublecat.Core.Assets.Fonts;
using Troublecat.Core;
using Troublecat.Utilities;
using Troublecat.Math;

namespace GameBootstrap.Scenes;

public class FaceScene : GameScene {
    private InternalTexture _face;
    private PixelFont _font;
    private float _rotationSpeed = 1f;

    private float _rotationAmount = 0f;

    public override void LoadContent(IDataLoader dataLoader) {
        _face = dataLoader.GetAsset<InternalTexture>("sprites/test-face");
        _font = dataLoader.GetAsset<PixelFont>("fonts/ChevyRay-Pinch");
    }

    public override void Update(Timing time) {
        base.Update(time);
        var perFrameRotation = time.Delta * _rotationSpeed;
        _rotationAmount += perFrameRotation;
    }

    public override void Draw(SpriteBatch spriteBatch) {
        DrawInternalTexture(spriteBatch, _face, new Vector2(BaseScreenSize.X/2, BaseScreenSize.Y/2 - 32), Vector2.One, Color.White, rotation: _rotationAmount);

        var rotationInRads = _rotationAmount/MathF.PI;
        var rotationAngle = Maths.FloorToInt(rotationInRads * 180) % 360;
        DrawFont($"Rotation: {rotationAngle}", _font, spriteBatch, (BaseScreenSize/2) + Directions.Down * 16, Vector2.One, Color.White);
    }
}
