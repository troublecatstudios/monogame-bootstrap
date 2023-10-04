using Microsoft.Xna.Framework;

namespace Troublecat;

public class World  {
    private readonly List<GameThing> _things = new();

    public IServiceProvider Services { get; set; }

    public List<GameThing> Things => _things;

    public void AddGameThing(GameThing thing) {
        _things.Add(thing);
    }

    public virtual void Update(GameTime gameTime) {
        foreach(var thing in _things) {
            thing.Update(gameTime);
        }
    }

    public virtual void Draw() {
        foreach(var thing in _things) {
            thing.Draw();
        }
    }
}
