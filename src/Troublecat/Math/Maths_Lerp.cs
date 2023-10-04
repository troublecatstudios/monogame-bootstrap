namespace Troublecat.Math;

public partial class Maths {
    public static float Lerp(float origin, float target, float factor)
    {
        return origin * (1 - factor) + target * factor;
    }
    public static int LerpInt(float origin, float target, float factor)
    {
        return RoundToInt(origin * (1 - factor) + target * factor);
    }

    public static double LerpSnap(float origin, float target, double factor, float threshold = 0.01f)
    {
        return System.Math.Abs(target - origin) < threshold ? target : origin * (1 - factor) + target * factor;
    }

    public static float LerpSnap(float origin, float target, float factor, float threshold = 0.01f)
    {
        return System.Math.Abs(target - origin) < threshold ? target : origin * (1 - factor) + target * factor;
    }
}
