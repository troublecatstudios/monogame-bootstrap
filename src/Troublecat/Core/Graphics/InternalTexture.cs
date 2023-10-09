namespace Troublecat.Core.Graphics;

public class InternalTexture {
    public AtlasCoordinates? Coordinates { get; private set; }
    public string? Texture2d { get; private set; }

    public InternalTexture(AtlasCoordinates atlasCoordinates)
    {
        Coordinates = atlasCoordinates;
        Texture2d = null;
    }

    public InternalTexture(string texture)
    {
        Coordinates = null;
        Texture2d = texture;
    }
}
