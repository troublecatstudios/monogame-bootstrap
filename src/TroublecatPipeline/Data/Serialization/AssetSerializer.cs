using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Troublecat.IO;

namespace Troublecat.Data.Serialization;

public class AssetSerializer {
    private readonly ILogger<AssetSerializer> _logger;
    internal readonly static JsonSerializerSettings _settings = new() {
        TypeNameHandling = TypeNameHandling.All,
        Formatting = Formatting.Indented,
        ContractResolver = new WritablePropertiesOnlyResolver(),
        MissingMemberHandling = MissingMemberHandling.Error,
        Error = SerializationHelper.HandleSerializationError,
        NullValueHandling = NullValueHandling.Ignore
    };
    private readonly static JsonSerializerSettings _compressedSettings = new() {
        TypeNameHandling = TypeNameHandling.All,
        ContractResolver = new WritablePropertiesOnlyResolver(),
        MissingMemberHandling = MissingMemberHandling.Error,
        Error = SerializationHelper.HandleSerializationError,
        NullValueHandling = NullValueHandling.Ignore
    };

    public AssetSerializer(ILogger<AssetSerializer> logger) {
        _logger = logger;
    }

    public string Serialize<T>(T value, bool isCompressed = false)
    {
        _logger.Verify(value != null, $"Cannot serialize a null {typeof(T).Name}");

        string json = JsonConvert.SerializeObject(value, isCompressed ? _compressedSettings : _settings);

        return json;
    }

    public ValueTask<string> SerializeAsync<T>(T value, bool isCompressed = false)
    {
        _logger.Verify(value != null, $"Cannot serialize a null {typeof(T).Name}");

        string json = JsonConvert.SerializeObject(value, isCompressed ? _compressedSettings : _settings);

        return ValueTask.FromResult(json);
    }

    public T? DeserializeGeneric<T>(string json, bool warnOnErrors = true) {
        T? asset = JsonConvert.DeserializeObject<T>(json, _settings);
        return asset;
    }
}
