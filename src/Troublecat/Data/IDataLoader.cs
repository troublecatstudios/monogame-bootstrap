using Microsoft.Xna.Framework.Graphics;

namespace Troublecat.Data;

public interface IDataLoader {

    event OnAfterAllContentLoadedEvent OnAfterAllContentLoaded;

    Task LoadContentProgress { get; }

    void LoadContent();

    T GetAsset<T>(string path) where T : class;
}
