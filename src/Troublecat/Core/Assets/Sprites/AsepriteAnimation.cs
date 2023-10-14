namespace Troublecat.Core.Assets.Sprites;

public class AsepriteAnimation {
    public string Name { get; set; } = string.Empty;
    public float Duration { get; set; }

    public List<AsepriteAnimationFrame> Frames { get; set; } = new();

    public bool StartAtRandomFrame { get; set;  }= false;

    public AnimationType Method = AnimationType.Looped;

}
