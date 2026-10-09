namespace MandelbrotExplorer.Structs;

public readonly struct DoubleDouble
{
    public double Hi { get; }
    public double Lo { get; }

    public DoubleDouble(double hi, double lo)
    {
        Hi = hi;
        Lo = lo;
    }

    public static DoubleDouble FromDouble(double value)
    {
        return new DoubleDouble(value, 0.0);
    }

    public static DoubleDouble Add(DoubleDouble a, DoubleDouble b)
    {
        double s = a.Hi + b.Hi;

        double v = s - a.Hi;

        double e =
            (a.Hi - (s - v)) +
            (b.Hi - v) +
            a.Lo +
            b.Lo;

        double hi = s + e;
        double lo = e - (hi - s);

        return new DoubleDouble(hi, lo);
    }

    public static DoubleDouble Subtract(DoubleDouble a, DoubleDouble b)
    {
        return Add(
            a,
            new DoubleDouble(-b.Hi, -b.Lo));
    }

    public static DoubleDouble Multiply(DoubleDouble a, double b)
    {
        double p = a.Hi * b;

        double e = Math.FusedMultiplyAdd(
            a.Hi,
            b,
            -p);

        e += a.Lo * b;

        double hi = p + e;
        double lo = e - (hi - p);

        return new DoubleDouble(hi, lo);
    }

    public static DoubleDouble operator +(DoubleDouble a, DoubleDouble b)
        => Add(a, b);

    public static DoubleDouble operator -(DoubleDouble a, DoubleDouble b)
        => Subtract(a, b);

    public static DoubleDouble operator *(DoubleDouble a, double b)
        => Multiply(a, b);

    public static DoubleDouble operator *(double b, DoubleDouble a)
        => Multiply(a, b);

    public (float Hi, float Lo) ToShaderPair()
    {
        float hi = (float)Hi;

        double residual =
            (Hi - (double)hi) +
            Lo;

        float lo = (float)residual;

        return (hi, lo);
    }
}