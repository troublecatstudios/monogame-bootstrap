
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Troublecat.Core.Diagnostics;
using Troublecat.Data.Importers.Aseprite;
using Troublecat.Data.Serialization;
using Troublecat.IO;

namespace Troublecat.Data.Importers;

[ResourceImporter("*.aseprite", "*.ase")]
public class AsepriteImporter : IResourceImporter {
    private readonly ILogger<AsepriteImporter> _logger;
    private readonly ImportConfiguration _configuration;
    private readonly AssetSerializer _serializer;

    public AsepriteImporter(ILogger<AsepriteImporter> logger, IOptions<ImportConfiguration> configuration, AssetSerializer serializer) {
        _logger = logger;
        _configuration = configuration.Value;
        _serializer = serializer;
    }

    public Task ImportAsync(string resourcePath) {
        using(_logger.BeginScope(new PerfScope())) {
            if (!Paths.Exists(resourcePath)) {
                _logger.LogError($"Resource not found! Does '{resourcePath}' exist?!");
                return Task.FromCanceled(default);
            }
            var packPath = Paths.GetPath(_configuration.ResourcesPackDirectoryAbsolute, "Sprites");
            var fileName = Path.GetFileNameWithoutExtension(resourcePath);
            AsepriteImportOptions options = new();
            _configuration.Sprites.TryGetValue(fileName, out options);
            _logger.LogInformation($"Importing {resourcePath}");
        }
        return Task.CompletedTask;
    }

}
