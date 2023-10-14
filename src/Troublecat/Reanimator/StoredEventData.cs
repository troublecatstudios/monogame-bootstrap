using Newtonsoft.Json;

namespace Troublecat.Reanimator;

[Serializable]
public class StoredEventData {
    private string _rawJson = string.Empty;

    private object? _instance;

    public void SetData(object? instance) {
        _rawJson = JsonConvert.SerializeObject(instance);
        _instance = null;
    }

    public object? GetData(Type type) {
        if (_instance == null) {
            if (_rawJson == null || _rawJson.Length == 0) {
                var created = Activator.CreateInstance(type);
                SetData(created);
            }
            _instance = JsonConvert.DeserializeObject(_rawJson!, type)!;
        }
        return _instance;
    }
}
