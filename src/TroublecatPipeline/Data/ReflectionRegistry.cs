
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Troublecat.Data;

public static class ReflectionRegistry {
    public static IServiceCollection AddImporters(this IServiceCollection services) {
        foreach(var asm in AppDomain.CurrentDomain.GetAssemblies()) {
            foreach(var type in asm.GetTypes().Where(t => (typeof(IResourceImporter).IsAssignableFrom(t)))) {
                var attr = type.GetCustomAttribute<ResourceImporterAttribute>();
                if (attr != null) {
                    services.AddSingleton(type);
                    services.AddSingleton(typeof(IResourceImporter), type);
                }
            }
        }
        return services;
    }
}
