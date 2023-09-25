using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Troublecat;

public class GameThing {
    private readonly IServiceProvider _provider;
    private readonly IList<IComponent> _components = new List<IComponent>();

    public GameThing(string name, IServiceProvider serviceProvider) {
        Name = name;
        _provider = serviceProvider;
    }

    public string Name { get; private set; }

    public Vector2 Position { get; set; }

    public T AddComponent<T>() where T : IComponent, new() {
        var component = new T();
        component.Initialize(this, _provider);
        _components.Add(component);
        return component;
    }

    public T GetComponent<T>() where T : IComponent {
        foreach(var c in _components) {
            if (c is T t) {
                return t;
            }
        }
        return default;
    }

    public virtual void Update(GameTime gameTime) {
        foreach(var c in _components) {
            c.Update(gameTime);
        }
    }

    public virtual void Draw() {
        foreach(var c in _components) {
            c.Draw();
        }
    }
}
