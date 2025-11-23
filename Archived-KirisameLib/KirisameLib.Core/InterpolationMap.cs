namespace KirisameLib;

public abstract class InterpolationMap
{
    //Getter
    public static InterpolationMap From(Func<double, double> map, bool easeIn = true) => new SimpleInterpolationMap(map, easeIn);


    //Ease mode
    public abstract Func<double, double> EaseIn { get; }
    public abstract Func<double, double> EaseOut { get; }
    public abstract Func<double, double> EaseInOut { get; }
    public abstract Func<double, double> EaseOutIn { get; }


    //Predefined

    /* algorithm reference:
     * https://github.com/godotengine/godot/blob/61598c5c88d95b96811d386cb20d714c35f4c6d7/scene/animation/easing_equations.h */
    public static InterpolationMap Linear => From(t => t);
    public static InterpolationMap Quadratic => From(t => t * t);
    public static InterpolationMap Cubic => From(t => t * t * t);
    public static InterpolationMap Quartic => From(t => t * t * t * t);
    public static InterpolationMap Quintic => From(t => t * t * t * t * t);
    public static InterpolationMap Exponential => From(t => Math.Pow(2, 10 * (t - 1)));
    public static InterpolationMap Sin => From(t => Math.Sin(t * Math.PI / 2), false);
    public static InterpolationMap Circle => From(t => 1 - Math.Sqrt(1 - t * t));
    public static InterpolationMap Back => From(t => t * t * ((1.70158 + 1) * t - 1.70158));
    public static InterpolationMap Elastic => From(t => 1 - Math.Cos(t * Math.PI * 2 / 0.3) * Math.Exp(-7 * t), false);
    public static InterpolationMap Spring => From(t =>
    {
        var s = 1 - t;
        return (Math.Sin(t * Math.PI * (0.2 + 2.5 * t * t * t)) * Math.Pow(s, 2.2) + t) * (1.0 + (1.2 * s));
    }, false); //wait
    public static InterpolationMap Bounce => From(t =>
    {
        const double n1 = 7.5625;
        const double d1 = 2.75;
        switch (t)
        {
            case < 1 / d1:
                return n1 * t * t;
            case < 2 / d1:
                t -= 1.5 / d1;
                return n1 * t * t + 0.75;
            case < 2.5 / d1:
                t -= 2.25 / d1;
                return n1 * t * t + 0.9375;
            default:
                t -= 2.625 / d1;
                return n1 * t * t + 0.984375;
        }
    }, false);
}

internal class SimpleInterpolationMap(Func<double, double> map, bool easeModeIn) : InterpolationMap
{
    private Func<double, double> Reverse() => t => 1 - map(1 - t);

    private Func<double, double> ForwardReverse() =>
        t => t < 0.5
                 ? map(2 * t) / 2
                 : 1 - map(2 - 2 * t) / 2;

    private Func<double, double> ReverseForward() =>
        t =>
            t < 0.5
                ? (1 - map(1 - 2 * t)) / 2
                : (1 + map(2 * t - 1)) / 2;

    public override Func<double, double> EaseIn => easeModeIn ? map : Reverse();
    public override Func<double, double> EaseOut => easeModeIn ? Reverse() : map;
    public override Func<double, double> EaseInOut => easeModeIn ? ForwardReverse() : ReverseForward();
    public override Func<double, double> EaseOutIn => easeModeIn ? ReverseForward() : ForwardReverse();
}