using Newtonsoft.Json;

namespace Troublecat.Data.Serialization;

public static class SerializationHelper
{
    /// <summary>
    /// This is really tricky. We currently use readonly structs everywhere, which is great
    /// and very optimized.
    /// HOWEVER, this is tricky in the editor serializer. We need an actual new copy
    /// so we don't modify any other IComponents which are using the same memory.
    /// In order to workaround that, we will literally serialize a new component and create its copy.
    /// </summary>
    public static T DeepCopy<T>(T c)
    {
        if (c == null) throw new ArgumentNullException(nameof(c));
        var settings = AssetSerializer._settings;
        if (JsonConvert.DeserializeObject(JsonConvert.SerializeObject(c, settings), c.GetType(), settings) is not T obj)
        {
            throw new InvalidOperationException($"Unable to serialize {c.GetType().Name} for editor!?");
        }

        return obj;
    }

    internal static void HandleSerializationError<T>(object? _, T e)
    {
        if (e is not Newtonsoft.Json.Serialization.ErrorEventArgs error ||
            error.ErrorContext.Member is not string memberName ||
            error.CurrentObject is null)
        {
            // We can't really do much about it :(
            return;
        }

        Type targetType = error.CurrentObject.GetType();

        error.ErrorContext.Handled = true;
    }
}
