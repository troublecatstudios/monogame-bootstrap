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
    Vector2 baseScreenSize = new Vector2(800, 480);
    private Matrix globalTransformation;
    int backbufferWidth, backbufferHeight;

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

    protected override void LoadContent()
    {
        _logger.LogInformation($"Loading content...");
        Loader.LoadContent();
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    public void ScalePresentationArea()
    {
        //Work out how much we need to scale our graphics to fill the screen
        backbufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
        backbufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
        float horScaling = backbufferWidth / baseScreenSize.X;
        float verScaling = backbufferHeight / baseScreenSize.Y;
        Vector3 screenScalingFactor = new Vector3(horScaling, verScaling, 1);
        globalTransformation = Matrix.CreateScale(screenScalingFactor);
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
        _spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null,null, globalTransformation);
        base.Draw(gameTime);
        _spriteBatch.End();
    }
}
