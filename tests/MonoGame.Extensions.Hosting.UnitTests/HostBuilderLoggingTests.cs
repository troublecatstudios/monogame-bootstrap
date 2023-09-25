using Microsoft.Extensions.Logging;

namespace MonoGame.Extensions.Hosting.UnitTests;

public class HostBuilderLoggingTests
{
    [Fact]
    public void GetValidILoggerByDefault()
    {
        var builder = GameApplication.CreateBuilder();
        var gameApp = builder.Build();

        ILogger logger = gameApp.Services.GetService<ILogger<HostBuilderLoggingTests>>();
        Assert.NotNull(logger);
        logger.LogError("An error");
    }

    [Fact]
    public void CanAddLoggingProviders()
    {
        var loggerProvider = new MyLoggerProvider();

        var builder = GameApplication.CreateBuilder();
        builder
            .Logging
            .Services
            .AddSingleton<ILoggerProvider>(loggerProvider);
        var gameApp = builder.Build();

        ILogger logger = gameApp.Services.GetService<ILogger<HostBuilderLoggingTests>>();
        logger.LogError("An error");
        Assert.Single(loggerProvider.Messages);
        Assert.Equal("An error", loggerProvider.Messages[0]);
    }

    private sealed class MyLoggerProvider : ILoggerProvider, ILogger
    {
        public List<string> Messages { get; } = new();

        public ILogger CreateLogger(string categoryName) => this;
        public IDisposable BeginScope<TState>(TState state) => this;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }

        public void Dispose() { }
    }
}
