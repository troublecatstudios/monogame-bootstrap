using Microsoft.Xna.Framework.Graphics;
using Troublecat.Core.Graphics;
using Troublecat.Reanimator;

namespace Troublecat.Core.Assets.Sprites;

public class AsepriteAsset {
    public string Name { get; set; } = string.Empty;
    public string AtlasTexturePath { get; set; } = string.Empty;
    public Dictionary<string, string> SecondaryTexturePaths { get; set; } = new();
    public List<Sprite> Sprites { get; set; } = new();
    public List<AsepriteAnimation> Animations { get; set; } = new();
    public List<AsepriteFileMetadata> Metadata { get; set; } = new();

    public bool TryGetAnimation(string animationName, out AsepriteAnimation? animation) {
        animation = null;
        foreach(var anim in Animations) {
            if (anim.Name.ToLowerInvariant().Equals(animationName.ToLowerInvariant())) {
                animation = anim;
                return true;
            }
        }
        return false;
    }
}
