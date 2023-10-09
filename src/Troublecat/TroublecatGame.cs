using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Troublecat.Data;

namespace Troublecat;

public class TroublecatGame : Microsoft.Xna.Framework.Game {
    private readonly ILogger<TroublecatGame> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IDataLoader? _dataLoader;

    private SpriteBatch _spriteBatch;
    protected Vector2 BaseScreenSize = new Vector2(320, 180);
    protected Vector3 ScreenScaling = Vector3.One;
    private Matrix globalTransformation;
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

    public void ScalePresentationArea()
    {
        //Work out how much we need to scale our graphics to fill the screen
        backbufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
        backbufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
        float horScaling = backbufferWidth / BaseScreenSize.X;
        float verScaling = backbufferHeight / BaseScreenSize.Y;
        ScreenScaling = new Vector3(horScaling, verScaling, 1);
        globalTransformation = Matrix.CreateScale(ScreenScaling);
        System.Diagnostics.Debug.WriteLine("Screen Size - Width[" + GraphicsDevice.PresentationParameters.BackBufferWidth + "] Height [" + GraphicsDevice.PresentationParameters.BackBufferHeight + "]");
    }

    protected override void Update(GameTime gameTime) {
        if (backbufferHeight != GraphicsDevice.PresentationParameters.BackBufferHeight ||
            backbufferWidth != GraphicsDevice.PresentationParameters.BackBufferWidth)
        {
            ScalePresentationArea();
        }
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null,null, globalTransformation);
        base.Draw(gameTime);

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