using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens;
using Troublecat.Data;
using Troublecat.Math;

using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using XnaTime = Microsoft.Xna.Framework.GameTime;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace Troublecat;

public class TroublecatGame : Microsoft.Xna.Framework.Game {
    private readonly ILogger<TroublecatGame> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IDataLoader? _dataLoader;

    private SpriteBatch _spriteBatch;
    protected Vector2 BaseScreenSize = new Vector2(320, 180);
    protected Vector2 ScreenScaling = Vector2.One;
    private XnaMatrix globalTransformation;
    int backbufferWidth, backbufferHeight;
    private bool _allContentDoneLoading;

    public TroublecatGame(ILogger<TroublecatGame> logger, IServiceProvider serviceProvider) {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected ILogger Logger => _logger;

    protected IDataLoader Loader {
        get {
            if (_dataLoader is null) {
                _dataLoader = _serviceProvider.GetRequiredService<IDataLoader>();
            }
            return _dataLoader;
        }
    }

    protected IServiceProvider ServiceProvider => _serviceProvider;

    protected override void LoadContent()
    {
        _logger.LogInformation($"Loading content...");
        Loader.LoadContent();
        Loader.OnAfterAllContentLoaded += InternalOnAfterAllContentLoaded;
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected virtual void OnAfterAllContentLoaded() {

    }

    protected virtual void InternalOnAfterAllContentLoaded() {
        OnAfterAllContentLoaded();
        _allContentDoneLoading = true;
    }

    protected override void Initialize() {
        base.Initialize();
        var graphics = _serviceProvider.GetRequiredService<Microsoft.Xna.Framework.GraphicsDeviceManager>();
        graphics.IsFullScreen = false;
        graphics.PreferredBackBufferWidth = Maths.FloorToInt(BaseScreenSize.X * ScreenScaling.X);
        graphics.PreferredBackBufferHeight = Maths.FloorToInt(BaseScreenSize.Y * ScreenScaling.Y);
        graphics.ApplyChanges();
    }

    public void ScalePresentationArea()
    {
        //Work out how much we need to scale our graphics to fill the screen
        backbufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
        backbufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
        float horScaling = backbufferWidth / BaseScreenSize.X;
        float verScaling = backbufferHeight / BaseScreenSize.Y;
        ScreenScaling = new Vector2(horScaling, verScaling);
        globalTransformation = XnaMatrix.CreateScale(new Vector3(ScreenScaling.X, ScreenScaling.Y, 1));
        System.Diagnostics.Debug.WriteLine("Screen Size - Width[" + GraphicsDevice.PresentationParameters.BackBufferWidth + "] Height [" + GraphicsDevice.PresentationParameters.BackBufferHeight + "]");
    }

    protected override void Update(XnaTime gameTime) {
        if (backbufferHeight != GraphicsDevice.PresentationParameters.BackBufferHeight ||
            backbufferWidth != GraphicsDevice.PresentationParameters.BackBufferWidth)
        {
            ScalePresentationArea();
        }
        base.Update(gameTime);
    }

    protected override void Draw(XnaTime gameTime) {
        base.Draw(gameTime);
        GraphicsDevice.Clear(XnaColor.CornflowerBlue);
        _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null,null, globalTransformation);

        if (!_allContentDoneLoading) {
            DrawLoading(_spriteBatch);
        } else {
            DrawSprites(_spriteBatch);
        }

        _spriteBatch.End();

    }

    protected virtual void DrawLoading(SpriteBatch batch) {

    }

    protected virtual void DrawSprites(SpriteBatch batch) {

    }
}
