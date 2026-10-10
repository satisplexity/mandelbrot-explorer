namespace MandelbrotExplorer.Structs;

/// <summary>
/// Represents a high-precision number as the sum of two double values:
/// High stores the leading component, and Low stores the correction.
/// </summary>
public readonly struct DoubleDouble
{
    /// <summary>
    /// Gets the leading component of the number.
    /// </summary>
    public double High { get; }

    /// <summary>
    /// Gets the low-order component that preserves additional precision.
    /// </summary>
    public double Low { get; }

    /// <summary>
    /// Initializes a number from its leading and low-order components.
    /// The components are stored without additional normalization.
    /// </summary>
    public DoubleDouble(double high, double low)
    {
        High = high;
        Low = low;
    }

    /// <summary>
    /// Creates a DoubleDouble from a double value with a zero low-order component.
    /// </summary>
    public static DoubleDouble FromDouble(double value)
        => new DoubleDouble(value, 0.0);

    /// <summary>
    /// Adds two numbers, accounting for the rounding error
    /// of the leading-component sum and both low-order components.
    /// </summary>
    public static DoubleDouble Add(DoubleDouble a, DoubleDouble b)
    {
        // Compute the sum of the leading components.
        double sum = a.High + b.High;

        // Compute an intermediate value used to recover the rounding error of the sum.
        double recoverValue = sum - a.High;

        // Recover the rounding error of the leading-component sum and include both low-order components.
        double error = (a.High - (sum - recoverValue)) + (b.High - recoverValue) + a.Low + b.Low;

        // Redistribute the result between the leading and low-order components.
        double high = sum + error;
        double low = error - (high - sum);

        return new DoubleDouble(high, low);
    }

    /// <summary>
    /// Subtracts the second number from the first.
    /// </summary>
    public static DoubleDouble Subtract(DoubleDouble a, DoubleDouble b)
        => Add(a, new DoubleDouble(-b.High, -b.Low));

    /// <summary>
    /// Multiplies a DoubleDouble by a double, accounting for
    /// the rounding error of the leading-component product.
    /// </summary>
    public static DoubleDouble Multiply(DoubleDouble a, double b)
    {
        // Compute the product of the leading component and the multiplier.
        double product = a.High * b;

        // FMA evaluates a.High * b - product with a single rounding,
        // recovering the rounding error of the original product.
        double error = Math.FusedMultiplyAdd(a.High, b, -product);

        // Include the contribution of the low-order component.
        error += a.Low * b;

        // Redistribute the result between the leading and low-order components.
        double high = product + error;
        double low = error - (high - product);

        return new DoubleDouble(high, low);
    }

    /// <summary>
    /// Adds two DoubleDouble values.
    /// </summary>
    public static DoubleDouble operator +(DoubleDouble a, DoubleDouble b)
        => Add(a, b);

    /// <summary>
    /// Subtracts the second DoubleDouble value from the first.
    /// </summary>
    public static DoubleDouble operator -(DoubleDouble a, DoubleDouble b)
        => Subtract(a, b);

    /// <summary>
    /// Multiplies a DoubleDouble by a double.
    /// </summary>
    public static DoubleDouble operator *(DoubleDouble a, double b)
        => Multiply(a, b);

    /// <summary>
    /// Multiplies a double by a DoubleDouble.
    /// </summary>
    public static DoubleDouble operator *(double b, DoubleDouble a)
        => Multiply(a, b);

    /// <summary>
    /// Converts the number into a pair of float values for shader use.
    /// The first value stores the leading component; the second stores a correction.
    /// This conversion does not preserve the full precision of the DoubleDouble.
    /// </summary>
    public (float High, float Low) ToShaderPair()
    {
        // Round the leading component to float precision.
        float high = (float)High;

        // Compute the residual lost during conversion
        // and include the original low-order component.
        double residual =
            (High - (double)high) +
            Low;

        // Store the residual in the second float.
        float low = (float)residual;

        return (high, low);
    }
}