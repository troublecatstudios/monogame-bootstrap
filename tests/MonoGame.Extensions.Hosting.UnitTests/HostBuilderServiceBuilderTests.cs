namespace MonoGame.Extensions.Hosting.UnitTests;

public class HostBuilderServiceBuilderTests
{
    [Fact]
    public void MultipleServicesAreRegisteredWithoutBuilder()
    {
        var builder = GameApplication.CreateBuilder();
        builder.Services.AddSingleton(new MappingService("key 1", "value 1"));
        builder.Services.AddSingleton(new MappingService("key 2", "value 2"));
        var gameApp = builder.Build();

        var mappingServices = gameApp.Services.GetServices<MappingService>().ToArray();

        Assert.Equal(2, mappingServices.Length);
        Assert.NotEqual(mappingServices[0], mappingServices[1]);

        Assert.Equal(new[] { "value 1" }, Get("key 1"));
        Assert.Equal(new[] { "value 2" }, Get("key 2"));
        Assert.Empty(Get("key 3"));

        IEnumerable<string> Get(string key)
        {
            foreach (var service in mappingServices)
                foreach (var value in service.Get(key))
                    yield return value;
        }
    }
}
