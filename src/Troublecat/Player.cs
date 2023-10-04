using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;
using MonoGame.Aseprite.Content.Processors;
using MonoGame.Aseprite.Sprites;

namespace Troublecat;

public class Player : GameThing {
    private readonly SpriteSheet _spriteSheet;
    private readonly AnimatedSprite _run;
    private readonly AnimatedSprite _idle;

    private AnimatedSprite _currentAnimation;
    private GraphicsDevice? _graphics;

    public Player(string name, IServiceProvider serviceProvider) : base(name, serviceProvider) {
        _graphics = serviceProvider.GetService<GraphicsDevice>();
        var aseFile = AsepriteFile.Load("./resources/Sprites/Ode-32.aseprite");
        _spriteSheet = SpriteSheetProcessor.Process(_graphics, aseFile);
        _run = _spriteSheet.CreateAnimatedSprite("Run");
        _idle = _spriteSheet.CreateAnimatedSprite("Idle");
    }

    public float MovementSpeed { get; set; } = 1f;

    public override void Update(GameTime gameTime) {
        var deltaTime = (float)gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond;
        var movement = Vector2.Zero;
        if (Keyboard.GetState().IsKeyDown(Keys.Right)) {
            movement += Vector2Helper.Right;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.Up)) {
            movement += Vector2Helper.Up;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.Left)) {
            movement += Vector2Helper.Left;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.Down)) {
            movement += Vector2Helper.Down;
        }

        if (movement == Vector2.Zero && _currentAnimation != _idle) {
            _currentAnimation = _idle;
        }
        if (movement != Vector2.Zero && _currentAnimation != _run) {
            _currentAnimation = _run;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.E)) MovementSpeed += 5 * deltaTime;
        if (Keyboard.GetState().IsKeyDown(Keys.Q)) MovementSpeed -= 5 * deltaTime;
        if (MovementSpeed < 0f) MovementSpeed = 0f;

        Position += movement * MovementSpeed * deltaTime;

        _currentAnimation.Update(gameTime);

        base.Update(gameTime);
    }

    public override void Draw() {
        base.Draw();
        var spriteBatch = new SpriteBatch(_graphics);

        //Just drawing the Sprite
        spriteBatch.Begin();
        _currentAnimation.Draw(spriteBatch, Position);
        spriteBatch.End();
    }
}
