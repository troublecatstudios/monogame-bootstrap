using Microsoft.Xna.Framework.Graphics;
using Troublecat.Core.Graphics;

namespace Troublecat.Core.Assets;

public class AsepriteDescriptor {
    public string Name { get; set; } = string.Empty;
    public int AnimationCount { get; set; }
    public int FrameCount { get; set; }
    public string AtlasTexturePath { get; set; } = string.Empty;
    public Dictionary<string, string> SecondaryTexturePaths { get; set; } = new();
    public List<AsepriteAtlasEntry> Entries { get; set; } = new();
    public List<AsepriteFileMetadata> Metadata { get; set; } = new();
}
