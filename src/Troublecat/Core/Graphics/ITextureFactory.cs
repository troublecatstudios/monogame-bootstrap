using Microsoft.Xna.Framework.Graphics;

namespace Troublecat.Core.Graphics;

public interface ITextureFactory {
    Texture2D[] GetAtlasTextures(AtlasId id, string name);
    AtlasTexture Create(AtlasId id, string name);
    Texture2D CreateTextureFromAtlas(AtlasCoordinates textureCoord, SurfaceFormat format = SurfaceFormat.Color, int scale = 1);
}
