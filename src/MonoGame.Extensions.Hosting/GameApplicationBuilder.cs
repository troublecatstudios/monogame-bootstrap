using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Xna.Framework;

namespace MonoGame.Extensions.Hosting;

// for reference, see:
// https://andrewlock.net/exploring-dotnet-6-part-3-exploring-the-code-behind-webapplicationbuilder/
/// <summary>
/// A builder for MonoGame applications and services.
/// </summary>
public sealed class GameApplicationBuilder
{
    private readonly GameApplicationServiceCollection _services = new();
    private readonly Lazy<ConfigurationManager> _configuration;
    private ILoggingBuilder? _logging;

    private readonly HostBuilder _hostBuilder = new();
    private readonly GameApplicationOptions _options;
    private GameApplication? _builtApplication;

    internal GameApplicationBuilder(GameApplicationOptions options, Action<IHostBuilder>? configureDefaults = null)
    {
        // Lazy-load the ConfigurationManager, so it isn't created if it is never used.
        // Don't capture the 'this' variable in AddSingleton, so GameApplicationBuilder can be GC'd.
        var configuration = new Lazy<ConfigurationManager>(() => new ConfigurationManager());
        Services.AddSingleton<IConfiguration>(_ => configuration.Value);

        _configuration = configuration;

        _options = options;
    }

    /// <summary>
    /// A collection of services for the application to compose. This is useful for adding user provided or framework provided services.
    /// </summary>
    public IServiceCollection Services => _services;

    /// <summary>
    /// A collection of configuration providers for the application to compose. This is useful for adding new configuration sources and providers.
    /// </summary>
    public ConfigurationManager Configuration => _configuration.Value;

    /// <summary>
    /// A collection of logging providers for the application to compose. This is useful for adding new logging providers.
    /// </summary>
    public ILoggingBuilder Logging
    {
        get
        {
            return _logging ??= InitializeLogging();

            ILoggingBuilder InitializeLogging()
            {
                // if someone accesses the Logging builder, ensure Logging has been initialized.
                Services.AddLogging();
                return new LoggingBuilder(Services);
            }
        }
    }

    public GameApplication Build()
    {
        ConfigureDefaultLogging();

        // Wire up the application configuration by copying the already built configuration providers over to final configuration builder.
        // We wrap the existing provider in a configuration source to avoid re-bulding the already added configuration sources.
        _hostBuilder.ConfigureAppConfiguration(builder =>
        {
            foreach (var source in ((IConfigurationBuilder)Configuration).Sources)
            {
                builder.Sources.Add(source);
            }

            foreach (var (key, value) in ((IConfigurationBuilder)Configuration).Properties)
            {
                builder.Properties[key] = value;
            }
        });

        // Copy the services that were added via GameApplicationBuilder.Services into the final IServiceCollection
        _hostBuilder.ConfigureServices((_, services) =>
        {
            foreach (var s in Services)
            {
                services.Add(s);
            }
        });

        // Mark the service collection as read-only to prevent future modifications
        _services.IsReadOnly = true;

        _builtApplication = new GameApplication(_hostBuilder.Build(), _services.BuildServiceProvider());

        return _builtApplication;
    }

    public GameApplicationBuilder UseGame<TGame>()
        where TGame : Game
    {
        Services.AddHostedService(serviceProvider =>
            new GameHostService(
                _options,
                _builtApplication!,
                serviceProvider.GetService<TGame>()!,
                serviceProvider.GetService<IHostApplicationLifetime>()!));

        Services.AddSingleton<TGame>();

        Services.AddSingleton(_ => GameHostService.Graphics!);
        Services.AddSingleton(_ => GameHostService.Graphics!.GraphicsDevice!);
        Services.AddSingleton(_ => GameHostService.ContentManager!);

        return this;
    }

    private void ConfigureDefaultLogging()
    {
        // By default, if no one else has configured logging, add a "no-op" LoggerFactory
        // and Logger services with no providers. This way when components try to get an
        // ILogger<> from the IServiceProvider, they don't get 'null'.
        Services.TryAdd(ServiceDescriptor.Singleton<ILoggerFactory, NullLoggerFactory>());
        Services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(NullLogger<>)));
    }

    private sealed class LoggingBuilder : ILoggingBuilder
    {
        public LoggingBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}
