using System.Numerics;
using Troublecat.Core.Geometry;

namespace Troublecat.Core.Assets;

public class AsepriteAtlasEntry {
    public Rectangle Rectangle { get; set; }
    public Vector2 Pivot { get; set; }
    public Vector4 Border { get; set; }
    public string Name { get; set; } = string.Empty;
    public int FrameIndex { get; set; }
}
