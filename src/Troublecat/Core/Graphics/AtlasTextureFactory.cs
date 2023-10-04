using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Troublecat.Configuration;
using Troublecat.Data.Serialization;
using Troublecat.Utilities;

namespace Troublecat.Core.Graphics;

public class AtlasTextureFactory : IAtlasTextureFactory {
    private readonly ILogger<AtlasTextureFactory> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TroublecatConfiguration _config;
    private readonly AssetSerializer _assetSerializer;
    private readonly Dictionary<AtlasId, AtlasTexture> _loadedAtlasses = new();

    public AtlasTextureFactory(ILogger<AtlasTextureFactory> logger, IOptions<TroublecatConfiguration> config, IServiceProvider serviceProvider, AssetSerializer assetSerializer) {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _config = config.Value;
        _assetSerializer = assetSerializer;
    }

    public AtlasTexture Create(AtlasId id, string name) {
        return new AtlasTexture(name, id, _serviceProvider.GetRequiredService<ITextureFactory>(), _serviceProvider.GetRequiredService<ILogger<AtlasTexture>>());
    }

    public AtlasTexture GetAtlas(AtlasId atlas, bool warnOnError = true)
    {
        if (atlas == AtlasId.None)
        {
            throw new ArgumentException("There's no atlas to fetch.");
        }

        if (!_loadedAtlasses.ContainsKey(atlas))
        {
            string filepath = Path.Join(_config.ResourcesPackDirectoryAbsolute, _config.AtlasFolderName, $"{atlas.GetDescription()}.json");
            AtlasTexture? newAtlas = _assetSerializer.DeserializeGeneric<AtlasTexture>(filepath, warnOnError);

            if (newAtlas is not null)
            {
                _loadedAtlasses[atlas] = newAtlas;
            }
            else
            {
                throw new ArgumentException($"Atlas {atlas} is not loaded and couldn't be loaded from '{filepath}'.");
            }
        }

        return _loadedAtlasses[atlas];
    }
}
