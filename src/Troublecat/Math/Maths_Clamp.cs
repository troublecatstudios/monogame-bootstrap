namespace Troublecat.Math;

public partial class Maths {
    public static float Clamp01(float v) => System.Math.Clamp(v, 0.0f, 1.0f);
    public static float Clamp01(int v) => System.Math.Clamp(v, 0, 1);

    public static float Clamp(float value, float min, float max) {
        return value < max ? value > min ? value : min : max;
    }
}
