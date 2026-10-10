using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows;

namespace MandelbrotExplorer.Effects;

/// <summary>
/// Provides a pixel shader effect for rendering the Mandelbrot fractal.
/// Exposes camera parameters and colors as dependency properties.
/// </summary>
public sealed class Mandelbrot : ShaderEffect
{
    /// <summary>
    /// Registers the compiled pixel shader as the default shader for this effect.
    /// </summary>
    static Mandelbrot()
        => PixelShaderProperty.OverrideMetadata(
            typeof(Mandelbrot),
            new UIPropertyMetadata(new PixelShader()
            {
                UriSource = new Uri(
                    "/MandelbrotExplorer;component/Shaders/Mandelbrot.ps",
                    UriKind.Relative)
            }));

    /// <summary>
    /// Initializes the input brush and disables padding around the effect.
    /// </summary>
    public Mandelbrot()
    {
        Input = new VisualBrush();

        // Keep the effect bounds aligned with the element bounds.
        PaddingTop = 0;
        PaddingBottom = 0;
        PaddingLeft = 0;
        PaddingRight = 0;
    }

    /// <summary>
    /// Gets or sets the input brush bound to shader sampler s0.
    /// </summary>
    public Brush Input
    {
        get => (Brush)GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }

    /// <summary>
    /// Identifies the Input dependency property and maps it to sampler s0.
    /// </summary>
    public static readonly DependencyProperty InputProperty
        = RegisterPixelShaderSamplerProperty(
            nameof(Input),
            typeof(Mandelbrot),
            0);

    #region Center

    /// <summary>
    /// Identifies the CenterHi dependency property and maps it to register c0.
    /// </summary>
    public static readonly DependencyProperty CenterHiProperty
        = DependencyProperty.Register(
            nameof(CenterHi),
            typeof(Point),
            typeof(Mandelbrot),
            new UIPropertyMetadata(
                new Point(0, 0),
                PixelShaderConstantCallback(0)));

    /// <summary>
    /// Gets or sets the leading components of the camera center coordinates.
    /// </summary>
    public Point CenterHi
    {
        get => (Point)GetValue(CenterHiProperty);
        set => SetValue(CenterHiProperty, value);
    }

    /// <summary>
    /// Identifies the CenterLo dependency property and maps it to register c1.
    /// </summary>
    public static readonly DependencyProperty CenterLoProperty
        = DependencyProperty.Register(
            nameof(CenterLo),
            typeof(Point),
            typeof(Mandelbrot),
            new UIPropertyMetadata(
                new Point(0, 0),
                PixelShaderConstantCallback(1)));

    /// <summary>
    /// Gets or sets the low-order corrections to the camera center coordinates.
    /// </summary>
    public Point CenterLo
    {
        get => (Point)GetValue(CenterLoProperty);
        set => SetValue(CenterLoProperty, value);
    }

    #endregion

    #region Scale

    /// <summary>
    /// Identifies the ScaleHi dependency property and maps it to register c2.
    /// </summary>
    public static readonly DependencyProperty ScaleHiProperty
        = DependencyProperty.Register(
            nameof(ScaleHi),
            typeof(float),
            typeof(Mandelbrot),
            new UIPropertyMetadata(
                1.0f,
                PixelShaderConstantCallback(2)));

    /// <summary>
    /// Gets or sets the leading component of the camera scale.
    /// Smaller scale values zoom further into the fractal.
    /// </summary>
    public float ScaleHi
    {
        get => (float)GetValue(ScaleHiProperty);
        set => SetValue(ScaleHiProperty, value);
    }

    /// <summary>
    /// Identifies the ScaleLo dependency property and maps it to register c3.
    /// </summary>
    public static readonly DependencyProperty ScaleLoProperty
        = DependencyProperty.Register(
            nameof(ScaleLo),
            typeof(float),
            typeof(Mandelbrot),
            new UIPropertyMetadata(
                0.0f,
                PixelShaderConstantCallback(3)));

    /// <summary>
    /// Gets or sets the low-order correction to the camera scale.
    /// </summary>
    public float ScaleLo
    {
        get => (float)GetValue(ScaleLoProperty);
        set => SetValue(ScaleLoProperty, value);
    }

    #endregion

    /// <summary>
    /// Identifies the Aspect dependency property and maps it to register c4.
    /// </summary>
    public static readonly DependencyProperty AspectProperty
        = DependencyProperty.Register(
            nameof(Aspect),
            typeof(float),
            typeof(Mandelbrot),
            new UIPropertyMetadata(
                1.0f,
                PixelShaderConstantCallback(4)));

    /// <summary>
    /// Gets or sets the viewport width-to-height ratio used for coordinate mapping.
    /// </summary>
    public float Aspect
    {
        get => (float)GetValue(AspectProperty);
        set => SetValue(AspectProperty, value);
    }

    #region Colors

    /// <summary>
    /// Identifies the BackgroundColor dependency property and maps it to register c5.
    /// </summary>
    public static readonly DependencyProperty BackgroundColorProperty
        = DependencyProperty.Register(
            nameof(BackgroundColor),
            typeof(Color),
            typeof(Mandelbrot),
            new UIPropertyMetadata(
                Colors.Black,
                PixelShaderConstantCallback(5)));

    /// <summary>
    /// Gets or sets the background color supplied to the shader.
    /// </summary>
    public Color BackgroundColor
    {
        get => (Color)GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    /// <summary>
    /// Identifies the EscapeColor dependency property and maps it to register c6.
    /// </summary>
    public static readonly DependencyProperty EscapeColorProperty
        = DependencyProperty.Register(
            nameof(EscapeColor),
            typeof(Color),
            typeof(Mandelbrot),
            new UIPropertyMetadata(
                Colors.White,
                PixelShaderConstantCallback(6)));

    /// <summary>
    /// Gets or sets the color supplied to the shader for escaped points.
    /// </summary>
    public Color EscapeColor
    {
        get => (Color)GetValue(EscapeColorProperty);
        set => SetValue(EscapeColorProperty, value);
    }

    /// <summary>
    /// Identifies the SetColor dependency property and maps it to register c7.
    /// </summary>
    public static readonly DependencyProperty SetColorProperty
        = DependencyProperty.Register(
            nameof(SetColor),
            typeof(Color),
            typeof(Mandelbrot),
            new UIPropertyMetadata(
                Colors.Black,
                PixelShaderConstantCallback(7)));

    /// <summary>
    /// Gets or sets the color supplied to the shader for points
    /// that do not escape within the iteration limit.
    /// </summary>
    public Color SetColor
    {
        get => (Color)GetValue(SetColorProperty);
        set => SetValue(SetColorProperty, value);
    }

    #endregion
}