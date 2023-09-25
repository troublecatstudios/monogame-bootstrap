using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Troublecat;

public class Player : GameThing {
    private readonly SpriteComponent _sprite;
    private readonly TileLoader _tileLoader;

    public Player(string name, IServiceProvider serviceProvider) : base(name, serviceProvider) {
        var contentManager = serviceProvider.GetService<ContentManager>();
        _sprite = AddComponent<SpriteComponent>();
        _tileLoader = new TileLoader(contentManager!);
        _sprite.Sprite = _tileLoader.GetTileByName("tile_0485");
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

        if (Keyboard.GetState().IsKeyDown(Keys.E)) MovementSpeed += 5 * deltaTime;
        if (Keyboard.GetState().IsKeyDown(Keys.Q)) MovementSpeed -= 5 * deltaTime;
        if (MovementSpeed < 0f) MovementSpeed = 0f;

        Position += movement * MovementSpeed * deltaTime;

        base.Update(gameTime);
    }
}
