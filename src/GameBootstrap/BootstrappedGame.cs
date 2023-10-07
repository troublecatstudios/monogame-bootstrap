using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Graphics;
using Troublecat;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace GameBootstrap;

public class BootstrappedGame : TroublecatGame {
    private SpriteBatch _spriteBatch;
    Vector2 baseScreenSize = new Vector2(800, 480);
    private XnaMatrix globalTransformation;
    int backbufferWidth, backbufferHeight;

    public BootstrappedGame(ILogger<BootstrappedGame> logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
    {
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        base.LoadContent();
    }

    public void ScalePresentationArea()
    {
        //Work out how much we need to scale our graphics to fill the screen
        backbufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
        backbufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
        float horScaling = backbufferWidth / baseScreenSize.X;
        float verScaling = backbufferHeight / baseScreenSize.Y;
        Vector3 screenScalingFactor = new Vector3(horScaling, verScaling, 1);
        globalTransformation = XnaMatrix.CreateScale(screenScalingFactor);
        System.Diagnostics.Debug.WriteLine("Screen Size - Width[" + GraphicsDevice.PresentationParameters.BackBufferWidth + "] Height [" + GraphicsDevice.PresentationParameters.BackBufferHeight + "]");
    }

    protected override void Update(Microsoft.Xna.Framework.GameTime gameTime) {
        if (backbufferHeight != GraphicsDevice.PresentationParameters.BackBufferHeight ||
            backbufferWidth != GraphicsDevice.PresentationParameters.BackBufferWidth)
        {
            ScalePresentationArea();
        }
        base.Update(gameTime);
    }

    protected override void Draw(Microsoft.Xna.Framework.GameTime gameTime) {
        GraphicsDevice.Clear(XnaColor.CornflowerBlue);
        _spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null,null, globalTransformation);
        base.Draw(gameTime);
        _spriteBatch.End();
    }
}
