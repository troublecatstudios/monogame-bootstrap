using System.Numerics;

namespace Troublecat.Aseprite.Utils;

internal static class Vector2Extensions {
    public static Vector2 Scale(this Vector2 vector, Vector2 scalingVector) => new(vector.X * scalingVector.X, vector.Y * scalingVector.Y);
}
