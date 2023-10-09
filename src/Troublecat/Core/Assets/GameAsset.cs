using Newtonsoft.Json;

namespace Troublecat.Core.Assets;

[Serializable]
public abstract class GameAsset {

    public string Name { get; set; } = string.Empty;

    [JsonProperty]
    public Guid Guid { get; protected set; }

    public void MakeGuid() {
        Guid = Guid.NewGuid();
    }

    public virtual void AfterDeserialized() { }
}
