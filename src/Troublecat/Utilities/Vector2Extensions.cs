using System.Numerics;
using Troublecat.Core.Geometry;
using Troublecat.Math;

namespace Troublecat.Utilities;

public static class Vector2Extensions {

    public static Vector2 Multiply(this Vector2 a, Microsoft.Xna.Framework.Vector2 b) => new(a.X * b.X, a.Y * b.Y);
    public static Point Ceiling(this Vector2 vector) => new(Maths.CeilToInt(vector.X), Maths.CeilToInt(vector.Y));

    public static Microsoft.Xna.Framework.Vector3 ToXnaVector3(this Vector2 vector) => new(vector.X, vector.Y, 0);
    public static Microsoft.Xna.Framework.Vector2 ToXnaVector2(this Vector2 vector) => new(vector.X, vector.Y);

    public static Point Floor(this Vector2 vector) => new(Maths.FloorToInt(vector.X), Maths.FloorToInt(vector.Y));
    public static Point Point(this Vector2 vector) => new(Maths.RoundToInt(vector.X), Maths.RoundToInt(vector.Y));
    public static Point Round(this Vector2 vector) => new(Maths.RoundToInt(vector.X), Maths.RoundToInt(vector.Y));

    /// <summary>
    /// Returns a new vector, rotated by the given angle. In radians.
    /// </summary>
    /// <param name="vector">The vector to rotate.</param>
    /// <param name="angle">The angle to rotate by.</param>
    /// <returns></returns>
    public static Vector2 Rotate(this Vector2 vector, float angle)
    {
        if (angle == 0) return vector;
        return new Vector2(
            (float)(vector.X * System.Math.Cos(angle) - vector.Y * System.Math.Sin(angle)),
            (float)(vector.X * System.Math.Sin(angle) + vector.Y * System.Math.Cos(angle))
        );
    }
}
