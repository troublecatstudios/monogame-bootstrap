namespace Troublecat.Math;

public partial class Maths {

    public const float Deg2Rad = (MathF.PI * 2) / 360;

    public static int FloorToInt(float v) => (int)MathF.Floor(v);

    public static int CeilToInt(float v) => (int)MathF.Ceiling(v);

    public static int RoundToInt(float v) => (int)MathF.Round(v);

    public static float Min(params float[] values) {
        var min = float.MaxValue;
        for (int i = 0; i < values.Length; i++) {
            if (values[i] < min)
                min = values[i];
        }

        return min;
    }

    public static float Max(params float[] values) {
        var max = float.MinValue;
        for (int i = 0; i < values.Length; i++) {
            if (values[i] > max)
                max = values[i];
        }

        return max;
    }
}
