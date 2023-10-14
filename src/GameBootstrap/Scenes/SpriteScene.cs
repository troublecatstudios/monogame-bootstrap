using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Troublecat.Core;
using Troublecat.Core.Assets.Fonts;
using Troublecat.Core.Assets.Sprites;
using Troublecat.Core.Graphics;
using Troublecat.Data;
using Troublecat.Input;
using Troublecat.Math;
using Troublecat.Utilities;

namespace GameBootstrap.Scenes;

public class SpriteScene : GameScene {
    private InputManager _inputManager;
    private AsepriteAsset _playerSprites;

    private AsepriteAnimation? _currentAnimation;
    private SpriteAnimator _animator = new();
    private float _playbackSpeed = 1f;

    private PixelFont _debugFont;

    public override void LoadContent(IDataLoader dataLoader) {
        _inputManager = new InputManager(1);
        _debugFont = dataLoader.GetAsset<PixelFont>("fonts/CabinCondensed-Regular");
        _playerSprites = dataLoader.GetAsset<AsepriteAsset>("sprites/Ode-32");
        if (_playerSprites.TryGetAnimation("idle", out var animation)) {
            _currentAnimation = animation!;
            AddComponent(_animator);
            _animator.SetAnimation(_currentAnimation);
        }
    }

    public override void Update(Timing time) {
        base.Update(time);
        _inputManager.Update(time);
        if (_inputManager.IsButtonDown(Keys.OemComma)) {
            _playbackSpeed -= 0.1f;
        }

        if (_inputManager.IsButtonDown(Keys.OemPeriod)) {
            _playbackSpeed += 0.1f;
        }
        _playbackSpeed = Maths.Clamp(_playbackSpeed, 0.1f, 5f);
        _animator.SetPlayback(_playbackSpeed);
    }

    public override void Draw(SpriteBatch spriteBatch) {
        var position = new Vector2(BaseScreenSize.X/2, BaseScreenSize.Y/2);
        if (_animator.CurrentSprite != null) {
            DrawSprite(spriteBatch, _animator.CurrentSprite, position, Vector2.One, Color.White);
        }
        DrawFont($"{_playbackSpeed}", _debugFont, spriteBatch, CenterScreen + Directions.Down * 32, Vector2.One, Color.White);
        base.Draw(spriteBatch);
    }
}
