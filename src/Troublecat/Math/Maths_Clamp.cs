namespace Troublecat.Math;

public partial class Maths {
    public static float Clamp01(float v) => System.Math.Clamp(v, 0.0f, 1.0f);
    public static float Clamp01(int v) => System.Math.Clamp(v, 0, 1);

    public static float Clamp(float value, float min, float max) {
        return value < max ? value > min ? value : min : max;
    }

    /// <summary>
    /// Loops the value t, so that it is never larger than length and never smaller than 0.
    /// </summary>
    /// <remarks>
    /// This is similar to the modulo operator but it works with floating point numbers. For example, using 3.0 for t and 2.5 for length, the result would be 0.5. With t = 5 and length = 2.5, the result would be 0.0. Note, however, that the behaviour is not defined for negative numbers as it is for the modulo operator.
    /// In the example below the value of time is restricted between 0.0 and just under 3.0. This is then used to keep the x position in this range.
    /// </remarks>
    /// <example>
    /// var position = new Vector3(Maths.Repeat(time, 3), y, z);
    /// </example>
    /// <param name="t"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static float Repeat(float t, float length) {
        return Clamp(t - Floor(t / length) * length, 0.0f, length);
    }
}
