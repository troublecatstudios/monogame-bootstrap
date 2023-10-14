namespace Troublecat.Core.Assets.Sprites;

[System.Serializable]
public class AsepriteAnimationSettings {

    public AsepriteAnimationSettings() {
    }

    public AsepriteAnimationSettings(string name) {
        AnimationName = name;
    }

    public string AnimationName { get; set; } = string.Empty;
    public bool IsLooped { get; set; } = true;
    public string About { get; set; } = string.Empty;

    public override string ToString() {
        return AnimationName;
    }
}
