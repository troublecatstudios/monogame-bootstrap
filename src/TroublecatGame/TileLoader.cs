using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Troublecat;

internal class TileLoader : IDisposable {
    private readonly ContentManager _content;

    public TileLoader(ContentManager content) {
        _content = content ?? throw new ArgumentNullException(nameof(content));
    }

    public void Dispose()
    {
        if (_content != null) {
            _content.Dispose();
        }
    }

    public Texture2D GetTileByName(string tileName) => _content.Load<Texture2D>($"Tiles/{tileName}");
}
