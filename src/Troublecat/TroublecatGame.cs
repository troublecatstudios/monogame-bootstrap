using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Troublecat.Data;

namespace Troublecat;

public class TroublecatGame : Game {
    private readonly ILogger<TroublecatGame> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IDataLoader? _dataLoader;

    public TroublecatGame(ILogger<TroublecatGame> logger, IServiceProvider serviceProvider) {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected ILogger Logger => _logger;

    protected IDataLoader Loader {
        get {
            if (_dataLoader is null) {
                _dataLoader = _serviceProvider.GetRequiredService<IDataLoader>();
            }
            return _dataLoader;
        }
    }

    protected override void LoadContent()
    {
        _logger.LogInformation($"Loading content...");
        Loader.LoadContent();
    }
}
