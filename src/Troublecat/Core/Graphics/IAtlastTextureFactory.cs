namespace Troublecat.Core.Graphics;

public interface IAtlasTextureFactory {
    AtlasTexture Create(AtlasId id, string name);
    AtlasTexture GetAtlas(AtlasId atlas, bool warnOnError = true);
}
