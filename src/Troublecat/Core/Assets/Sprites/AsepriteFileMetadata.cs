namespace Troublecat.Core.Assets.Sprites;

[Serializable]
public class AsepriteFileMetadata {
    public string Name { get; set; } = string.Empty;
    public List<string> Args { get; set; } = new();
}
