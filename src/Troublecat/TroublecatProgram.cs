using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Troublecat.Hosting;
using Serilog;
using Troublecat.Data;
using Troublecat.Data.Serialization;
using Troublecat.Configuration;
using Troublecat.Core.Graphics;
using Troublecat.Core.Rendering;
using Troublecat.IO;

namespace Troublecat;

public class TroublecatProgram<TGame> where TGame : TroublecatGame {
    private readonly string[]? _args;

    public TroublecatProgram(string[]? args = null) {
        _args = args;
    }

    private IHost BuildGameHost() {
        return Host.CreateDefaultBuilder(_args)
            .ConfigureAppConfiguration((cfg) => cfg.AddJsonFile(Paths.GetPath("troublecat.json")))
            .ConfigureLogging((context, cfg) => {
                cfg.ClearProviders();
                var logger = new LoggerConfiguration()
                                .ReadFrom.Configuration(context.Configuration)
                                .CreateLogger();
                cfg.AddSerilog(logger);
            })
            .ConfigureServices((context, services) => {
                // configure any dependency injection here...
                ConfigureServicesInternal(context, services);
                ConfigureServices(context, services);
            })
            .Build();
    }

    private void ConfigureServicesInternal(HostBuilderContext context, IServiceCollection services) {
        services.Configure<TroublecatConfiguration>(context.Configuration.GetSection("Core"));
        services.AddHostedService(serviceProvider =>
            new GameHostService(
                serviceProvider.GetService<TGame>()!,
                serviceProvider.GetService<IHostApplicationLifetime>()!));

        services.AddSingleton<TGame>();
        services.AddSingleton<AssetSerializer>();
        services.AddSingleton<IRenderService, RenderService>();
        services.AddSingleton<IAtlasTextureFactory, AtlasTextureFactory>();
        services.AddSingleton<ITextureFactory, TextureFactory>();
        services.AddSingleton<IDataLoader, GameDataLoader>();

        services.AddSingleton(_ => GameHostService.Graphics!);
        services.AddSingleton(_ => GameHostService.Graphics!.GraphicsDevice!);
        services.AddSingleton(_ => GameHostService.ContentManager!);
    }

    protected virtual void ConfigureServices(HostBuilderContext context, IServiceCollection services) {

    }

    public async Task RunAsync() {
        using(var host = BuildGameHost()) {
            await host.RunAsync(CancellationToken.None);
        }
    }
}
