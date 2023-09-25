using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace MonoGame.Extensions.Hosting.IntegrationTests;

public class HostApplicationLifetimeTests
{
    private const string NAME = nameof(NAME);
    private const string EXPECTED = nameof(EXPECTED);
    private string Actual { get; set; } = default!;

    [FactSkipWhenDebuggerIsAttached]
    public async Task Ensure_that_OnStarted_is_invoked_when_not_null()
    {
        // Arrange
        void action() => Actual = EXPECTED;

        var options = new GameApplicationOptions
        {
            OnStarted = _ => action()
        };

        var builder = GameApplication.CreateBuilder(options).UseGame<MockGame>();

        using var gameApp = builder.Build();

        // Act
        await gameApp.RunAsync();

        // Assert
        Actual.Should().Be(EXPECTED);
    }

    [FactSkipWhenDebuggerIsAttached]
    public async Task Ensure_that_OnStopping_is_invoked_when_not_null()
    {
        // Arrange
        void action() => Actual = EXPECTED;

        var options = new GameApplicationOptions
        {
            OnStopping = _ => { action(); }
        };

        var builder = GameApplication.CreateBuilder(options).UseGame<MockGame>();

        using var gameApp = builder.Build();

        // Act
        await gameApp.RunAsync();

        // Assert
        Actual.Should().Be(EXPECTED);
    }

    [FactSkipWhenDebuggerIsAttached]
    public async Task Ensure_that_OnStopped_is_invoked_when_not_null()
    {
        // Arrange
        void action() => Actual = EXPECTED;

        var options = new GameApplicationOptions
        {
            OnStopped = _ => { action(); }
        };

        var builder = GameApplication.CreateBuilder(options).UseGame<MockGame>();

        using var gameApp = builder.Build();

        // Act
        await gameApp.RunAsync();

        // Assert
        Actual.Should().Be(EXPECTED);
    }

    class MockGame : Game
    {
        private static readonly MethodInfo MethodToReplace = typeof(Game).GetMethod("Run", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, new Type[] { typeof(GameRunBehavior) })!;
        private static readonly MethodInfo MethodToInject = new Action<GameRunBehavior>(_ =>
        {
            GameHostService.HostService!.StopAsync(CancellationToken.None);
        }).Method;

        static MockGame()
        {
            Injection.Replace(MethodToReplace, MethodToInject);
        }
    }
}
