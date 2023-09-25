namespace MonoGame.Extensions.Hosting;

/// <summary>
/// Options for configuring the behavior for CreateBuilder(GameApplicationOptions).
/// </summary>
public sealed class GameApplicationOptions
{
    public string[]? Args { get; init; }

    public Action<GameApplication>? OnStarted { get; init; }
    public Action<GameApplication>? OnStopping { get; init; }
    public Action<GameApplication>? OnStopped { get; init; }
}
