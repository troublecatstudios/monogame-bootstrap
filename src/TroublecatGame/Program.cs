
using System;
using Serilog;
using MonoGame.Extensions.Hosting;
using Troublecat;

try {
    InitializeLogging();
    var options = new GameApplicationOptions() {
        OnStopping = (app) => {
            Log.Information("Shutting down");
        }
    };
    var builder = GameApplication.CreateBuilder(args).UseGame<TroublecatGame>();
    builder.Logging.AddSerilog( dispose: true );
    Log.Information("Starting up...");
    using var game = builder.Build();
    await game.RunAsync();
} catch (Exception ex) {
    Log.Error(ex, "Whoops! Something went wrong.");
}

void InitializeLogging() {
    Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/game.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
}
