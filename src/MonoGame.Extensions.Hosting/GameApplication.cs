using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MonoGame.Extensions.Hosting;

// Based on:
// https://github.com/dotnet/maui/blob/main/src/Core/src/Hosting/MauiApp.cs
/// <summary>
/// Game application used to configure the game and host services.
/// </summary>
public sealed class GameApplication : IDisposable, IAsyncDisposable
{
    private readonly IHost _host;

    internal GameApplication(IHost host, IServiceProvider services)
    {
        _host = host;
        Services = services;
    }

    /// <summary>
    /// The application's configured services.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// The application's configured <see cref="IConfiguration"/>.
    /// </summary>
    public IConfiguration Configuration => Services.GetRequiredService<IConfiguration>();

    public static GameApplicationBuilder CreateBuilder(params string[] args) => new(new GameApplicationOptions { Args = args });

    public static GameApplicationBuilder CreateBuilder(GameApplicationOptions options) => new(options);

    public void Run()
    {
        HostingAbstractionsHostExtensions.Run(_host);
    }

    public Task RunAsync(CancellationToken cancellationToken = default)
    {
        return HostingAbstractionsHostExtensions.RunAsync(_host, cancellationToken);
    }

    public void Dispose()
    {
        // Explicitly dispose the Configuration, since it is added as a singleton object that the ServiceProvider
        // won't dispose.
        (Configuration as IDisposable)?.Dispose();

        (Services as IDisposable)?.Dispose();

        _host.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (Services is IAsyncDisposable asyncSvcDisposable)
        {
            // Fire and forget because this is called from a sync context
            await asyncSvcDisposable.DisposeAsync();
        }
        else
        {
            (Services as IDisposable)?.Dispose();
        }

        if (Configuration is IAsyncDisposable asyncCfgDisposable)
        {
            // Fire and forget because this is called from a sync context
            await asyncCfgDisposable.DisposeAsync();
        }
        else
        {
            (Configuration as IDisposable)?.Dispose();
        }
    }
}
