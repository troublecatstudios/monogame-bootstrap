namespace Troublecat.Core;

public readonly struct Timing {

    public static Timing FromGameTime(Microsoft.Xna.Framework.GameTime gameTime) {
        return new Timing() {
            Delta = (float)gameTime.ElapsedGameTime.TotalSeconds,
            FixedDelta = (float)gameTime.ElapsedGameTime.TotalSeconds,
            Total = (float)gameTime.TotalGameTime.TotalSeconds,
        };
    }

    public Timing() {

    }

    public readonly float Delta { get; init; } = 0f;
    public readonly float FixedDelta { get; init; } = 0f;
    public readonly float Total { get; init; } = 0f;

    public static implicit operator Timing(Microsoft.Xna.Framework.GameTime gameTime) {
        return Timing.FromGameTime(gameTime);
    }
}
