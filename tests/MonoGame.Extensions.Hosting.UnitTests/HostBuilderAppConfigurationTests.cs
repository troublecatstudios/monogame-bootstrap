using Microsoft.Extensions.Configuration;

namespace MonoGame.Extensions.Hosting.UnitTests;

public class HostBuilderAppConfigurationTests
{
    [Fact]
    public void CanConfigureAppConfiguration()
    {
        var builder = GameApplication.CreateBuilder();
        builder
            .Configuration
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                    { "key 1", "value 1" },
            });
        var gameApp = builder.Build();

        var configuration = gameApp.Services.GetRequiredService<IConfiguration>();

        Assert.Equal("value 1", configuration["key 1"]);
    }

    [Fact]
    public void AppConfigurationOverwritesValues()
    {
        var builder = GameApplication.CreateBuilder();
        builder
            .Configuration
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                    { "key 1", "value 1" },
                    { "key 2", "value 2" },
            });

        builder
            .Configuration
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                    { "key 1", "value a" },
            });

        var gameApp = builder.Build();

        var configuration = gameApp.Services.GetRequiredService<IConfiguration>();

        Assert.Equal("value a", configuration["key 1"]);
        Assert.Equal("value 2", configuration["key 2"]);
    }

    [Fact]
    public void ConfigureServicesCanUseConfig()
    {
        var builder = GameApplication.CreateBuilder();
        builder
            .Configuration
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                    { "key 1", "value 1" },
            });

        Assert.Equal("value 1", builder.Configuration["key 1"]);
    }
}