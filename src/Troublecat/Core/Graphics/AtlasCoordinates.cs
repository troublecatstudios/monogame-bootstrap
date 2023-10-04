using Microsoft.Xna.Framework.Graphics;
using Troublecat.Core.Geometry;

namespace Troublecat.Core.Graphics;

public readonly struct AtlasCoordinates {
    public Point AtlasSize => new(0, 0);
    public int Width => SourceRectangle.Width;
    public int Height => SourceRectangle.Height;

    public readonly int AtlasIndex;
    public readonly string Name;
    public readonly Point Size;
    public readonly IntRectangle SourceRectangle;
    public readonly Rectangle UV;
    public readonly IntRectangle TrimArea;
    public readonly AtlasId AtlasId;

    public static AtlasCoordinates Empty = new();

    public AtlasCoordinates(string name, AtlasId atlasId, IntRectangle atlasRectangle, IntRectangle trimArea, Point size, int atlasIndex, int atlasWidth, int atlasHeight)
    {
        (Name, SourceRectangle, TrimArea, AtlasIndex) = (name, atlasRectangle, trimArea, atlasIndex);
        AtlasId = atlasId;
        Size = size;

        UV = new Rectangle((float)SourceRectangle.X / atlasWidth, (float)SourceRectangle.Y / atlasHeight, (float)SourceRectangle.Width / atlasWidth, (float)SourceRectangle.Height / atlasHeight);
    }
}
