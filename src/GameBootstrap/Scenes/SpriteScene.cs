using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using Troublecat.Core;
using Troublecat.Core.Assets.Sprites;
using Troublecat.Core.Graphics;
using Troublecat.Data;
using Troublecat.Input;

namespace GameBootstrap.Scenes;

public class SpriteScene : GameScene {
    private InputManager _inputManager;
    private AsepriteAsset _playerSprites;

    private AsepriteAnimation? _currentAnimation;

    public override void LoadContent(IDataLoader dataLoader) {
        _inputManager = new InputManager(1);
        _playerSprites = dataLoader.GetAsset<AsepriteAsset>("sprites/Ode-32");
        if (_playerSprites.TryGetAnimation("idle", out var animation)) {
            _currentAnimation = animation!;
        }
    }

    public override void Update(Timing time) {
        base.Update(time);
    }

    public override void Draw(SpriteBatch spriteBatch) {
        var position = new Vector2(BaseScreenSize.X/2, BaseScreenSize.Y/2);
        if (_currentAnimation != null) {
            var frame = _currentAnimation.Frames[0];
            DrawSprite(spriteBatch, frame.Sprite!, position, Vector2.One, Color.White);
        }
        base.Draw(spriteBatch);
    }
}
