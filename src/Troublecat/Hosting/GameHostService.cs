using Microsoft.Extensions.Hosting;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;

namespace Troublecat.Hosting;

// for reference, see:
// https://andrewlock.net/introducing-ihostlifetime-and-untangling-the-generic-host-startup-interactions/
internal sealed class GameHostService : IHostedService {
    // Used to stop the host service in the integration tests
    // TODO: Replace this property with a CancellationTokenSource to allow cancelling executing task from provided token. This would allow us to run the integration tests in parallel.
    // https://github.com/dotnet/extensions/issues/3218
    // https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Hosting.Abstractions/src/BackgroundService.cs
    internal static IHostedService? HostService { get; private set; }

    private readonly TroublecatGame _game;
    private readonly IHostApplicationLifetime _appLifetime;

    public GameHostService(TroublecatGame game, IHostApplicationLifetime appLifetime) {
        _game = game ?? throw new ArgumentNullException(nameof(game));
        _appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));

        var graphicsManager = new GraphicsDeviceManager(game);
        if (game.Services.GetService<IGraphicsDeviceService>() is not null) {
            game.Services.RemoveService(typeof(IGraphicsDeviceService));
        }
        game.Services.AddService<IGraphicsDeviceService>(graphicsManager);
        Graphics = graphicsManager;

        ((IGraphicsDeviceManager)graphicsManager).CreateDevice();

        ContentManager = game.Content;

        HostService = this;
    }

    internal static GraphicsDeviceManager? Graphics { get; private set; }

    internal static ContentManager? ContentManager { get; private set; }

    internal static Thread? GameThread { get; private set; }

    public Task StartAsync(CancellationToken cancellationToken) {
        // _appLifetime.ApplicationStarted.Register(OnStarted);
        // _appLifetime.ApplicationStopping.Register(OnStopping);
        // _appLifetime.ApplicationStopped.Register(OnStopped);

        _game.Exiting += OnGameExiting;

        _ = RunGameInternalAsync();

        return Task.CompletedTask;
    }

    private Task RunGameInternalAsync() {
        try {
            _game.Run();
        } catch {
            _appLifetime.StopApplication();
            throw;
        }
        return Task.CompletedTask;
    }

    private void OnGameExiting(object? sender, EventArgs e) => StopAsync(new CancellationToken());

    public Task StopAsync(CancellationToken cancellationToken) {
        _appLifetime.StopApplication();

        return Task.CompletedTask;
    }
}
