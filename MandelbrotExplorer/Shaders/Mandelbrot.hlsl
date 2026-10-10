// Input sampler reserved for the WPF effect.
// The fractal is generated procedurally, so this sampler is not used.
sampler2D input : register(s0);

// Leading components of the camera center coordinates.
float2 CenterHigh : register(c0);

// Low-order corrections to the camera center coordinates.
float2 CenterLow : register(c1);

// Leading and low-order components of the camera scale.
float ScaleHigh : register(c2);
float ScaleLow : register(c3);

// Viewport width-to-height ratio.
float Aspect : register(c4);

// Colors used for the escape-time gradient and non-escaping points.
float4 BackgroundColor : register(c5);
float4 EscapeColor     : register(c6);
float4 SetColor        : register(c7);

float MaxIterations : register(c8);

// Extended-precision values are represented as float2(high, low).
// Each pair approximates a single number as high + low.
// This is float-pair arithmetic, not native double-precision arithmetic.

/// <summary>
/// Computes a sum and its rounding residual.
/// Requires abs(a) >= abs(b) for the error-free transformation.
/// </summary>
float2 ddQuickTwoSum(float a, float b)
{
    float s = a + b;
    float e = b - (s - a);

    return float2(s, e);
}

/// <summary>
/// Computes a sum and its rounding residual without requiring
/// a particular ordering of the operand magnitudes.
/// </summary>
float2 ddTwoSum(float a, float b)
{
    float s = a + b;

    // Recover the portion of the rounded sum attributed to b.
    float v = s - a;

    // Recover the rounding residual from both operands.
    float e = (a - (s - v)) + (b - v);

    return float2(s, e);
}

/// <summary>
/// Redistributes a pair into leading and low-order components.
/// Assumes the leading component dominates in magnitude.
/// </summary>
float2 ddNormalize(float2 a)
{
    return ddQuickTwoSum(a.x, a.y);
}

/// <summary>
/// Adds two extended-precision values.
/// </summary>
float2 ddAdd(float2 a, float2 b)
{
    // Add the leading components and recover their rounding residual.
    float2 s = ddTwoSum(a.x, b.x);

    // Include both low-order components.
    float e = s.y + a.y + b.y;

    // Renormalize the result.
    float2 r = ddQuickTwoSum(s.x, e);

    return r;
}

/// <summary>
/// Subtracts the second extended-precision value from the first.
/// </summary>
float2 ddSub(float2 a, float2 b)
{
    // Negate both components before adding.
    float2 nb = float2(-b.x, -b.y);

    return ddAdd(a, nb);
}

/// <summary>
/// Multiplies two extended-precision values using split products
/// to recover the rounding residual of the leading-component product.
/// </summary>
float2 ddMul(float2 a, float2 b)
{
    // Splitting constant for a float with a 24-bit significand: 2^12 + 1.
    const float SPLIT = 4097.0;

    float ah = a.x;
    float al = a.y;

    float bh = b.x;
    float bl = b.y;

    // Split the first leading component into two smaller components.
    float ca = SPLIT * ah;

    float ahHi = ca - (ca - ah);
    float ahLo = ah - ahHi;

    // Split the second leading component in the same way.
    float cb = SPLIT * bh;

    float bhHi = cb - (cb - bh);
    float bhLo = bh - bhHi;

    // Compute the rounded product of the leading components.
    float p = ah * bh;

    // Recover its rounding residual from the split products.
    float e =
        ((ahHi * bhHi - p) + ahHi * bhLo + ahLo * bhHi)
        + ahLo * bhLo;

    // Include the cross products and the low-order product.
    e += ah * bl;
    e += al * bh;
    e += al * bl;

    // Renormalize the result.
    float2 r = ddQuickTwoSum(p, e);

    return r;
}

/// <summary>
/// Multiplies an extended-precision value by a single float.
/// </summary>
float2 ddMulFloat(float2 a, float b)
{
    return ddMul(a, float2(b, 0.0));
}

/// <summary>
/// Computes the square of an extended-precision value.
/// </summary>
float2 ddSqr(float2 a)
{
    return ddMul(a, a);
}

/// <summary>
/// Maps the pixel to the complex plane, evaluates the Mandelbrot iteration,
/// and returns a color based on whether and when the orbit escapes.
/// </summary>
float4 main(float2 uv : TEXCOORD) : COLOR
{
    // Map texture coordinates from [0, 1] to [-1, 1].
    float2 ndc = uv * 2.0 - 1.0;

    // Make the vertical axis increase upward.
    ndc.y = -ndc.y;

    // Reconstruct the camera parameters as extended-precision pairs.
    float2 centerX = float2(CenterHigh.x, CenterLow.x);
    float2 centerY = float2(CenterHigh.y, CenterLow.y);
    float2 scale = float2(ScaleHigh, ScaleLow);

    // Correct the horizontal offset for the viewport aspect ratio.
    float offsetX = ndc.x * Aspect;
    float offsetY = ndc.y;

    // Compute the complex coordinate c = cr + i * ci for this pixel.
    float2 cr = ddAdd(centerX, ddMulFloat(scale, offsetX));
    float2 ci = ddAdd(centerY, ddMulFloat(scale, offsetY));

    // Start the Mandelbrot orbit at z = 0.
    float2 zr = float2(0.0, 0.0);
    float2 zi = float2(0.0, 0.0);

    // Record whether the orbit escaped and its zero-based escape iteration.
    float escaped = 0.0;
    float escapeIteration = (float)MaxIterations;

    int iterationLimit = (int)max(1.0, MaxIterations);

    [loop]
    for (int i = 0; i < iterationLimit; i++)
    {
        // Freeze the orbit after escape, preserving its final value
        // for smooth coloring.
        if (escaped < 0.5)
        {
            float2 zr2 = ddSqr(zr);
            float2 zi2 = ddSqr(zi);
            float2 zrzi = ddMul(zr, zi);

            // Evaluate zNext = z^2 + c:
            // real      = zr^2 - zi^2 + cr
            // imaginary = 2 * zr * zi + ci
            float2 nextR = ddAdd(ddSub(zr2, zi2), cr);
            float2 twoZRZI = ddAdd(zrzi, zrzi);
            float2 nextI = ddAdd(twoZRZI, ci);

            zr = nextR;
            zi = nextI;

            // Approximate |z|^2 using only the leading components.
            float radius2 =
                zr.x * zr.x +
                zi.x * zi.x;

            // An orbit escapes when its magnitude exceeds 2.
            if (radius2 > 4.0)
            {
                escaped = 1.0;
                escapeIteration = (float)i;
            }
        }
    }

    // Non-escaping points are treated as belonging to the set
    // at the current iteration limit.
    if (escaped < 0.5)
    {
        return SetColor;
    }

    // Estimate the magnitude of the orbit at escape.
    float radius2 = zr.x * zr.x + zi.x * zi.x;

    // Keep the magnitude above 1 so the nested logarithm is defined.
    float magnitude = sqrt(max(radius2, 1.000001));

    // Apply a logarithmic correction to the escape iteration
    // to reduce visible color bands.
    float smoothIteration =
        escapeIteration +
        1.0 -
        log(log(magnitude)) / log(2.0);

    // Normalize the coloring value and clamp it to [0, 1].
    float t = saturate(smoothIteration / (float)iterationLimit);

    // Blend between the two exterior colors.
    return lerp(BackgroundColor, EscapeColor, t);
}