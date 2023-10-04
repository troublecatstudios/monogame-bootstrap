using System;

namespace Troublecat;

public static class ServiceProviderExtensions {
    public static T? GetService<T>(this IServiceProvider provider) {
        var service = provider.GetService(typeof(T));
        if (service == null) return default;
        return (T)service;
    }
}
