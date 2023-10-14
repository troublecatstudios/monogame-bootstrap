using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Graphics;
using Troublecat;
using Microsoft.Xna.Framework.Input;
using GameBootstrap.Scenes;

namespace GameBootstrap;

public class BootstrappedGame : TroublecatGame {
    private bool _lastQPressed = false;
    private bool _lastEPressed = false;

    private int _currentSceneIndex = 0;
    private readonly List<GameScene> AllScenes = new() {
        new FaceScene(),
        new TextScene(),
        new SpriteScene(),
    };


    public BootstrappedGame(ILogger<BootstrappedGame> logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
    {
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        ScreenScaling = Vector2.One * 4;
    }

    protected override void LoadContent() {
        base.LoadContent();
        foreach(var scene in AllScenes) {
            scene.BaseScreenSize = BaseScreenSize;
        }
    }

    protected override void OnAfterAllContentLoaded() {
        foreach(var scene in AllScenes) {
            scene.ConfigureServices(ServiceProvider);
            scene.LoadContent(Loader);
        }
    }

    protected override void Update(Microsoft.Xna.Framework.GameTime gameTime) {
        base.Update(gameTime);
        UpdateInput();
        AllScenes[_currentSceneIndex].Update(gameTime);
    }

    protected override void DrawSprites(SpriteBatch batch) {
        base.DrawSprites(batch);
        AllScenes[_currentSceneIndex].Draw(batch);
    }

    private void UpdateInput() {
        var kbState = Keyboard.GetState();
        if (kbState.IsKeyDown(Keys.E) && !_lastEPressed) {
            _currentSceneIndex++;
            if (_currentSceneIndex >= AllScenes.Count) {
                _currentSceneIndex = 0;
            }
        }
        if (kbState.IsKeyDown(Keys.Q) && !_lastQPressed) {
            _currentSceneIndex--;
            if (_currentSceneIndex < 0) {
                _currentSceneIndex = AllScenes.Count - 1;
            }
        }
        _lastEPressed = kbState.IsKeyDown(Keys.E);
        _lastQPressed = kbState.IsKeyDown(Keys.Q);
    }
}
