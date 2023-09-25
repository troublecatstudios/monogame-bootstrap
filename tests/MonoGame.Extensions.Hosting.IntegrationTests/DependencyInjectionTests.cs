using FluentAssertions.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MonoGame.Extensions.Hosting.IntegrationTests;

public class DependencyInjectionTests
{
    private const string NAME = nameof(NAME);
    private const string EXPECTED = nameof(EXPECTED);

    private string Actual { get; set; } = default!;

    [FactSkipWhenDebuggerIsAttached]
    public async Task Ensure_that_dependencies_are_injected_in_ctor()
    {
        // Arrange
        void action() => Actual = EXPECTED;

        var builder = GameApplication.CreateBuilder().UseGame<MockGame>();
        builder.Services.AddSingleton<IMyDependency>(_ = new MyDependency { Action = action });

        using var gameApp = builder.Build();

        // Act
        await gameApp.RunAsync();

        // Assert
        Actual.Should().Be(EXPECTED);
    }

    interface IMyDependency
    {
        Action Action { get; set; }
    }

    class MyDependency : IMyDependency
    {
        public Action Action { get; set; } = new Action(() => { });
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
            RuntimeHelpers.PrepareMethod(MethodToReplace.MethodHandle);
            RuntimeHelpers.PrepareMethod(MethodToInject.MethodHandle);
            Injection.Replace(MethodToReplace, MethodToInject);
        }

        public MockGame(IMyDependency myDependency)
        {
            myDependency.Action?.Invoke();
        }
    }
}