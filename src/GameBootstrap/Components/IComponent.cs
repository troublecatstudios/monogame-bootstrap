using Microsoft.Xna.Framework.Graphics;
using Troublecat.Core;

namespace GameBootstrap.Components;

public interface IComponent {

}

public interface IUpdateableComponent : IComponent {
    void Update(Timing time);
}

public interface IDrawableComponent : IComponent {
    void Draw(SpriteBatch spriteBatch);
}
