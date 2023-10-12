using System.Numerics;
using System.Text.RegularExpressions;
using Troublecat.Aseprite.Chunks;

namespace Troublecat.Aseprite;

public enum MetadataType {
    Unknown,
    Transform,
    SecondaryTexture,
    Arguments
};

public class Metadata {
    static public string MetadataCharPrefix = "@";

    public MetadataType Type { get; private set; }

    public int LayerIndex { get; private set; }

    public LayerChunk Layer { get; private set; }

    //Average position per frames
    public Dictionary<int, Vector2> Transforms { get; private set; }

    public List<string> Args { get; private set; }

    public Metadata(LayerChunk layer, int layerIndex) {

        Layer = layer;
        LayerIndex = layerIndex;
        Args = new List<string>();
        Transforms = new Dictionary<int, Vector2>();

        var layerName = layer.LayerName;

        var regex = new Regex("@transform\\(\"(.*)\"\\)");
        var match = regex.Match(layer.LayerName);
        if (match.Success) {
            Type = MetadataType.Transform;
            Args.Add(match.Groups[1].Value);
            return;
        }

        // Check if secondary texture layer
        regex = new Regex("@secondary\\((.*)\\)");
        match = regex.Match(layerName);
        if (match.Success) {
            Type = MetadataType.SecondaryTexture;
            Args.Add(match.Groups[1].Value);
            return;
        }

        // Check if it's a shortcut for some common secondary textures
        if (layerName.Equals("@emission", StringComparison.OrdinalIgnoreCase)) {
            Type = MetadataType.SecondaryTexture;
            Args.Add("_Emission");
            return;
        }
        if (layerName.Equals("@normal", StringComparison.OrdinalIgnoreCase)) {
            Type = MetadataType.SecondaryTexture;
            Args.Add("_NormalMap");
            return;
        }
        if (layerName.Equals("@mask", StringComparison.OrdinalIgnoreCase)) {
            Type = MetadataType.SecondaryTexture;
            Args.Add("_MaskTex");
            return;
        }

        regex = new Regex("@(\\w+)\\((.*)\\)");
        match = regex.Match(layerName);
        if (match.Success) {
            Args.Add($"name={match.Groups[1].Value}");
            Type = MetadataType.Arguments;
            Args.AddRange(match.Groups[2].Value.Split(' '));
            return;
        }
    }
}
