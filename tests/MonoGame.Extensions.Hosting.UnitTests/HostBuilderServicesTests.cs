using Microsoft.Extensions.Logging;

namespace MonoGame.Extensions.Hosting.UnitTests;

/// <remarks>
/// Based on:
/// https://github.com/dotnet/maui/blob/main/src/Core/tests/UnitTests/Hosting/HostBuilderServicesTests.cs
/// </remarks>
public class HostBuilderServicesTests
{
    [Fact]
    public void CanGetServices()
    {
        var builder = GameApplication.CreateBuilder();
        var gameApp = builder.Build();

        Assert.NotNull(gameApp.Services);
    }

    [Fact]
    public void GetServiceThrowsWhenConstructorParamTypesWereNotRegistered()
    {
        var builder = GameApplication.CreateBuilder();
        builder.Services.AddTransient<IFooBarService, FooBarService>();
        var gameApp = builder.Build();

        Assert.Throws<InvalidOperationException>(() => gameApp.Services.GetService<IFooBarService>());
    }

    [Fact]
    public void GetServiceThrowsWhenNoPublicConstructors()
    {
        var builder = GameApplication.CreateBuilder();
        builder.Services.AddTransient<IFooService, BadFooService>();
        var gameApp = builder.Build();

        var ex = Assert.Throws<InvalidOperationException>(() => gameApp.Services.GetService<IFooService>());
        Assert.Contains("suitable constructor", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GetServiceHandlesFirstOfMultipleConstructors()
    {
        var builder = GameApplication.CreateBuilder();
        builder.Services.AddTransient<IFooService, FooService>();
        builder.Services.AddTransient<IFooBarService, FooDualConstructor>();
        var gameApp = builder.Build();

        var service = gameApp.Services.GetService<IFooBarService>();

        var foobar = Assert.IsType<FooDualConstructor>(service);
        Assert.IsType<FooService>(foobar.Foo);
    }

    [Fact]
    public void GetServiceHandlesSecondOfMultipleConstructors()
    {
        var builder = GameApplication.CreateBuilder();
        builder.Services.AddTransient<IBarService, BarService>();
        builder.Services.AddTransient<IFooBarService, FooDualConstructor>();
        var mauiApp = builder.Build();

        var service = mauiApp.Services.GetService<IFooBarService>();

        var foobar = Assert.IsType<FooDualConstructor>(service);
        Assert.IsType<BarService>(foobar.Bar);
    }

    [Fact]
    public void GetServiceHandlesUsesCorrectCtor_DefaultWithNothing()
    {
        var builder = GameApplication.CreateBuilder();
        builder.Services.AddTransient<IFooBarService, FooTrioConstructor>();
        var mauiApp = builder.Build();

        var service = mauiApp.Services.GetService<IFooBarService>();

        var trio = Assert.IsType<FooTrioConstructor>(service);
        Assert.Null(trio.Foo);
        Assert.Null(trio.Bar);
        Assert.Null(trio.Cat);
        Assert.Equal("()", trio.Option);
    }

    [Fact]
    public void GetServiceHandlesUsesCorrectCtor_DefaultWithBar()
    {
        var builder = GameApplication.CreateBuilder();
        builder.Services.AddTransient<IBarService, BarService>();
        builder.Services.AddTransient<IFooBarService, FooTrioConstructor>();
        var gameApp = builder.Build();

        var service = gameApp.Services.GetService<IFooBarService>();

        var trio = Assert.IsType<FooTrioConstructor>(service);
        Assert.Null(trio.Foo);
        Assert.Null(trio.Bar);
        Assert.Null(trio.Cat);
        Assert.Equal("()", trio.Option);
    }

    [Fact]
    public void GetServiceHandlesUsesCorrectCtor_Foo()
    {
        var builder = GameApplication.CreateBuilder();
        builder.Services.AddTransient<IFooService, FooService>();
        builder.Services.AddTransient<IFooBarService, FooTrioConstructor>();
        var gameApp = builder.Build();

        var service = gameApp.Services.GetService<IFooBarService>();

        var trio = Assert.IsType<FooTrioConstructor>(service);
        Assert.IsType<FooService>(trio.Foo);
        Assert.Null(trio.Bar);
        Assert.Null(trio.Cat);
        Assert.Equal("(Foo)", trio.Option);
    }

    [Fact]
    public void GetServiceHandlesUsesCorrectCtor_FooWithCat()
    {
        var builder = GameApplication.CreateBuilder();
        builder.Services.AddTransient<IFooService, FooService>();
        builder.Services.AddTransient<ICatService, CatService>();
        builder.Services.AddTransient<IFooBarService, FooTrioConstructor>();
        var gameApp = builder.Build();

        var service = gameApp.Services.GetService<IFooBarService>();

        var trio = Assert.IsType<FooTrioConstructor>(service);
        Assert.IsType<FooService>(trio.Foo);
        Assert.Null(trio.Bar);
        Assert.Null(trio.Cat);
        Assert.Equal("(Foo)", trio.Option);
    }

    [Fact]
    public void GetServiceHandlesUsesCorrectCtor_FooBar()
    {
        var builder = GameApplication.CreateBuilder();
        builder.Services.AddTransient<IFooService, FooService>();
        builder.Services.AddTransient<IBarService, BarService>();
        builder.Services.AddTransient<IFooBarService, FooTrioConstructor>();
        var gameApp = builder.Build();

        var service = gameApp.Services.GetService<IFooBarService>();

        var trio = Assert.IsType<FooTrioConstructor>(service);
        Assert.IsType<FooService>(trio.Foo);
        Assert.IsType<BarService>(trio.Bar);
        Assert.Null(trio.Cat);
        Assert.Equal("(Foo, Bar)", trio.Option);
    }

    [Fact]
    public void GetServiceHandlesUsesCorrectCtor_FooBarCat()
    {
        var builder = GameApplication.CreateBuilder();
        builder.Services.AddTransient<IFooService, FooService>();
        builder.Services.AddTransient<IBarService, BarService>();
        builder.Services.AddTransient<ICatService, CatService>();
        builder.Services.AddTransient<IFooBarService, FooTrioConstructor>();
        var gameApp = builder.Build();

        var service = gameApp.Services.GetService<IFooBarService>();

        var trio = Assert.IsType<FooTrioConstructor>(service);
        Assert.IsType<FooService>(trio.Foo);
        Assert.IsType<BarService>(trio.Bar);
        Assert.IsType<CatService>(trio.Cat);
        Assert.Equal("(Foo, Bar, Cat)", trio.Option);
    }

    [Fact]
    public void GetServiceCanReturnTypesThatHaveConstructorParams()
    {
        var builder = GameApplication.CreateBuilder();
        builder.Services.AddTransient<IFooService, FooService>();
        builder.Services.AddTransient<IBarService, BarService>();
        builder.Services.AddTransient<IFooBarService, FooBarService>();
        var gameApp = builder.Build();

        var service = gameApp.Services.GetService<IFooBarService>();

        var foobar = Assert.IsType<FooBarService>(service);
        Assert.IsType<FooService>(foobar.Foo);
        Assert.IsType<BarService>(foobar.Bar);
    }

    [Fact]
    public void GetServiceCanReturnTypesThatHaveUnregisteredConstructorParamsButHaveDefaultValues()
    {
        var builder = GameApplication.CreateBuilder();
        builder.Services.AddTransient<IFooBarService, FooDefaultValueConstructor>();
        var gameApp = builder.Build();

        var foo = gameApp.Services.GetService<IFooBarService>();

        Assert.NotNull(foo);

        var actual = Assert.IsType<FooDefaultValueConstructor>(foo);

        Assert.Null(actual.Bar);
    }

    [Fact]
    public void GetServiceCanReturnTypesThatHaveRegisteredConstructorParamsAndHaveDefaultValues()
    {
        var builder = GameApplication.CreateBuilder();
        builder.Services.AddTransient<IBarService, BarService>();
        builder.Services.AddTransient<IFooBarService, FooDefaultValueConstructor>();
        var gameApp = builder.Build();

        var foo = gameApp.Services.GetService<IFooBarService>();

        Assert.NotNull(foo);

        var actual = Assert.IsType<FooDefaultValueConstructor>(foo);

        Assert.NotNull(actual.Bar);
        Assert.IsType<BarService>(actual.Bar);
    }

    [Fact]
    public void GetServiceCanReturnTypesThatHaveSystemDefaultValues()
    {
        var builder = GameApplication.CreateBuilder();
        builder.Services.AddTransient<IFooBarService, FooDefaultSystemValueConstructor>();
        var gameApp = builder.Build();

        var foo = gameApp.Services.GetService<IFooBarService>();

        Assert.NotNull(foo);

        var actual = Assert.IsType<FooDefaultSystemValueConstructor>(foo);

        Assert.Equal("Default Value", actual.Text);
    }

    [Fact]
    public void GetServiceCanReturnEnumerableParams()
    {
        var builder = GameApplication.CreateBuilder();
        builder.Services.AddTransient<IFooService, FooService>();
        builder.Services.AddTransient<IFooService, FooService2>();
        builder.Services.AddTransient<IFooBarService, FooEnumerableService>();
        var gameApp = builder.Build();

        var service = gameApp.Services.GetService<IFooBarService>();
        var foobar = Assert.IsType<FooEnumerableService>(service);

        var serviceTypes = foobar.Foos
            .Select(s => s.GetType().FullName)
            .ToArray();
        Assert.Contains(typeof(FooService).FullName, serviceTypes);
        Assert.Contains(typeof(FooService2).FullName, serviceTypes);
    }

    [Fact]
    public void WillRetrieveDifferentTransientServices()
    {
        var builder = GameApplication.CreateBuilder();
        builder.Services.AddTransient<IFooService, FooService>();
        var gameApp = builder.Build();

        AssertTransient<IFooService, FooService>(gameApp.Services);
    }

    [Fact]
    public void WillRetrieveSameSingletonServices()
    {
        var builder = GameApplication.CreateBuilder();
        builder.Services.AddSingleton<IFooService, FooService>();
        var gameApp = builder.Build();

        AssertSingleton<IFooService, FooService>(gameApp.Services);
    }

    [Fact]
    public void WillRetrieveMixedServices()
    {
        var builder = GameApplication.CreateBuilder();
        builder.Services.AddSingleton<IFooService, FooService>();
        builder.Services.AddTransient<IBarService, BarService>();
        var gameApp = builder.Build();

        AssertSingleton<IFooService, FooService>(gameApp.Services);
        AssertTransient<IBarService, BarService>(gameApp.Services);
    }

    [Fact]
    public void WillRetrieveEnumerables()
    {
        var builder = GameApplication.CreateBuilder();
        builder.Services.AddTransient<IFooService, FooService>();
        builder.Services.AddTransient<IFooService, FooService2>();
        var gameApp = builder.Build();

        var fooServices = gameApp.Services
            .GetServices<IFooService>()
            .ToArray();
        Assert.Equal(2, fooServices.Length);

        var serviceTypes = fooServices
            .Select(s => s.GetType().FullName)
            .ToArray();
        Assert.Contains(typeof(FooService).FullName, serviceTypes);
        Assert.Contains(typeof(FooService2).FullName, serviceTypes);
    }

    [Fact]
    public void CanCreateLogger()
    {
        var builder = GameApplication.CreateBuilder();
        builder.Services.AddLogging(logging => logging.AddConsole());
        var gameApp = builder.Build();

        var factory = gameApp.Services.GetRequiredService<ILoggerFactory>();

        var logger = factory.CreateLogger<HostBuilderServicesTests>();

        Assert.NotNull(logger);
    }

    private static void AssertTransient<TInterface, TConcrete>(IServiceProvider services)
    {
        var service1 = services.GetService<TInterface>();

        Assert.NotNull(service1);
        Assert.IsType<TConcrete>(service1);

        var service2 = services.GetService<TInterface>();

        Assert.NotNull(service2);
        Assert.IsType<TConcrete>(service2);

        Assert.NotEqual(service1, service2);
    }

    private static void AssertSingleton<TInterface, TConcrete>(IServiceProvider services)
    {
        var service1 = services.GetService<TInterface>();

        Assert.NotNull(service1);
        Assert.IsType<TConcrete>(service1);

        var service2 = services.GetService<TInterface>();

        Assert.NotNull(service2);
        Assert.IsType<TConcrete>(service2);

        Assert.Equal(service1, service2);
    }
}