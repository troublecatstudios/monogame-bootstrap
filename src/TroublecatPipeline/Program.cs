using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Troublecat;
using Troublecat.Data;
using Troublecat.Data.Serialization;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((cfg) => cfg.AddJsonFile("troublecat.json"))
    .ConfigureLogging((context, cfg) => {
        cfg.ClearProviders();
        var logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(context.Configuration)
                        .CreateLogger();
        cfg.AddSerilog(logger);
    })
    .ConfigureServices((context, services) => {
        services.Configure<ImportConfiguration>(context.Configuration.GetSection("Importing"));
        services.AddScoped<ProcessPipelineProgram>();
        services.AddSingleton<AssetSerializer>();
        services.AddImporters();
    })
    .Build();

using var scope = host.Services.CreateScope();
var pipeline = scope.ServiceProvider.GetRequiredService<ProcessPipelineProgram>();
await pipeline.RunAsync();
