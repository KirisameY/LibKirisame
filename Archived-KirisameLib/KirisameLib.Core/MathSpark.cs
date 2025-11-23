using System.Numerics;

namespace KirisameLib;

public static class MathSpark
{
    //Extension of max,min,clamp
    public static T Max<T>(T a, T b)
        where T : INumber<T> =>
        a > b ? a : b;

    public static T Min<T>(T a, T b)
        where T : INumber<T> =>
        a < b ? a : b;

    public static T Clamp<T>(T value, T min, T max)
        where T : INumber<T> =>
        Max(Min(value, max), min);

    public static int Round(float value) => (int)(value + 0.5f);
    public static int Round(double value) => (int)(value + 0.5d);


    //Mix with given interpolation
    public static double Mix(double a, double b, double t, Func<double, double> interpolation) =>
        a + (b - a) * interpolation(t);

    public static float Mix(float a, float b, double t, Func<double, double> interpolation) =>
        a + (b - a) * (float)interpolation(t);

    public static T Mix<T>(T a, T b, double t, Func<double, double> interpolation, Func<T, T, double, T> lerp) =>
        lerp(a, b, interpolation(t));
}