using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Troublecat;

public interface IComponent {
    void Initialize(GameThing parent, IServiceProvider provider);
    void Update(GameTime gameTime);
    void Draw();
}
