using System.Numerics;

namespace Troublecat.Utilities;

public static class Vector2Extensions {
    public static Microsoft.Xna.Framework.Vector3 ToXnaVector3(this Vector2 vector) => new(vector.X, vector.Y, 0);
    public static Microsoft.Xna.Framework.Vector2 ToXnaVector2(this Vector2 vector) => new(vector.X, vector.Y);
}
