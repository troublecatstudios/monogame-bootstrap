using Troublecat.Core.Assets;
using Troublecat.Core.Assets.Sprites;
using Troublecat.Core.Graphics;

namespace Troublecat.Reanimator;

[Serializable]
public class AnimationFrame {
    public Sprite? Sprite { get; set; }
    public float FrameDuration = 1f;
    public List<ReanimatorEvent> Events { get; set; } = new();
    public List<StoredEventData> EventData { get; set; } = new();
    public List<String> Markers { get; set; } = new();

    public bool HasMarker(string marker) {
        if (Markers == null || Markers.Count == 0) return false;
        return Markers.Any(s => s.ToLowerInvariant().Equals(marker.ToLowerInvariant()));
    }
}
