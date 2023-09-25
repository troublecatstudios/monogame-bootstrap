using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Troublecat;

public class SpriteComponent : IComponent
{
    private GameThing _parent;
    private IServiceProvider _provider;

    private SpriteBatch _batch;

    public Texture2D Sprite { get; set; }

    public void Draw()
    {
        if (_batch == null) {
            var device = _provider.GetService<GraphicsDevice>();
            _batch = new SpriteBatch(device);
        }
        _batch.Begin();
        _batch.Draw(Sprite, _parent.Position, Color.White);
        _batch.End();
    }

    public void Initialize(GameThing parent, IServiceProvider provider)
    {
        _provider = provider;
        _parent = parent;
    }

    public void Update(GameTime gameTime)
    {

    }
}
