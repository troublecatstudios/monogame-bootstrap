using System.Numerics;

namespace Troublecat.Core.Geometry;

public struct Vector2Int {
    public Vector2Int(int x, int y) {
        X = x;
        Y = y;
    }

    public int X { get; set; }
    public int Y { get; set; }

    public static implicit operator Vector2(Vector2Int vector) => new Vector2(vector.X, vector.Y);
    public static implicit operator Vector2Int(Vector2 vector) => new Vector2Int((int)vector.X, (int)vector.Y);
}
