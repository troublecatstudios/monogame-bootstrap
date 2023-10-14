namespace Troublecat.Reanimator;

public class SpriteAnimation {

    public string Name { get; set; } = string.Empty;
    public float Duration { get; set; }

    public List<AnimationFrame> Frames { get; set; } = new();

    public bool StartAtRandomFrame { get; set;  }= false;

    public SpriteAnimationType Method = SpriteAnimationType.Looped;

    public bool HasMarker(string marker) {
        if (Frames == null || Frames.Count == 0) return false;
        for (var i = 0; i < Frames.Count; i++) {
            if (Frames[i].HasMarker(marker)) return true;
        }
        return false;
    }
}
