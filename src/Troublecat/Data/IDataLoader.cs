using Microsoft.Xna.Framework.Graphics;

namespace Troublecat.Data;

public interface IDataLoader {
    void LoadContent();

    T GetAsset<T>(string path) where T : class;
}
