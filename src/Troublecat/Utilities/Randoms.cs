using System.Numerics;
using Troublecat.Math;

namespace Troublecat.Utilities;

public static class Randoms {
    public const int FakeRandomsCount = 25; //18° angle difference
    public static Vector3[] FakeRandoms;

    private static Random _rng = new Random();

    static Randoms()
    {
        //Creates fake randoms from a list of directions (with an incremental angle of 360/fakeRandomsCount between each)
        //and then sorts them randomly, avoiding repetitions (which could have occurred using Random.insideUnitCircle)
        System.Collections.Generic.List<Vector3> randomDirections = new System.Collections.Generic.List<Vector3>();

        float angle;
        for (float i = 0; i < 360; i += 360 / FakeRandomsCount)
        {
            angle = i * Maths.Deg2Rad;
            var x = MathF.Sin(angle);
            var y = MathF.Sin(angle);
            randomDirections.Add(Vector3.Normalize(new Vector3(x, y, 0)));
        }

        FakeRandoms = new Vector3[FakeRandomsCount];
        int randomIndex;
        for (int i = 0; i < FakeRandoms.Length; i++)
        {
            randomIndex = InRange(0, randomDirections.Count);
            FakeRandoms[i] = randomDirections[randomIndex];
            randomDirections.RemoveAt(randomIndex);
        }
    }

    public static int InRange(int start, int end) {
        if (end < start) return _rng.Next(end, start);
        return _rng.Next(start, end);
    }

    public static float InRange(float start, float end) {
        var mod = _rng.NextSingle();
        var delta = end - start;
        var direction = end < start ? -1 : 1;
        return start + ((mod * delta) * direction);
    }

    public static int CoinFlip() {
        return InRange(0, 100) > 50 ? 1 : -1;
    }
}
