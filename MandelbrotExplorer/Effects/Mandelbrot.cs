using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MandelbrotExplorer.Effects;

public sealed class Mandelbrot : ShaderEffect
{
    private static readonly PixelShader Shader = new()
    {
        UriSource = new Uri("/MandelbrotExplorer;component/Shaders/Mandelbrot.ps", UriKind.Relative)
    };

    static Mandelbrot()
    {
        PixelShaderProperty.OverrideMetadata(
            typeof(Mandelbrot),
            new UIPropertyMetadata(Shader));
    }

    public Mandelbrot()
    {
        Input = new VisualBrush();

        PaddingTop = 0;
        PaddingBottom = 0;
        PaddingLeft = 0;
        PaddingRight = 0;
    }

    public Brush Input
    {
        get => (Brush)GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }

    public static readonly DependencyProperty InputProperty =
        RegisterPixelShaderSamplerProperty(
            nameof(Input),
            typeof(Mandelbrot),
            0);

    public Point CenterHi
    {
        get => (Point)GetValue(CenterHiProperty);
        set => SetValue(CenterHiProperty, value);
    }

    public static readonly DependencyProperty CenterHiProperty =
        DependencyProperty.Register(
            nameof(CenterHi),
            typeof(Point),
            typeof(Mandelbrot),
            new UIPropertyMetadata(
                new Point(0, 0),
                PixelShaderConstantCallback(0)));

    public Point CenterLo
    {
        get => (Point)GetValue(CenterLoProperty);
        set => SetValue(CenterLoProperty, value);
    }

    public static readonly DependencyProperty CenterLoProperty =
        DependencyProperty.Register(
            nameof(CenterLo),
            typeof(Point),
            typeof(Mandelbrot),
            new UIPropertyMetadata(
                new Point(0, 0),
                PixelShaderConstantCallback(1)));

    public float ScaleHi
    {
        get => (float)GetValue(ScaleHiProperty);
        set => SetValue(ScaleHiProperty, value);
    }

    public static readonly DependencyProperty ScaleHiProperty =
        DependencyProperty.Register(
            nameof(ScaleHi),
            typeof(float),
            typeof(Mandelbrot),
            new UIPropertyMetadata(
                1.0f,
                PixelShaderConstantCallback(2)));


    public float ScaleLo
    {
        get => (float)GetValue(ScaleLoProperty);
        set => SetValue(ScaleLoProperty, value);
    }

    public static readonly DependencyProperty ScaleLoProperty =
        DependencyProperty.Register(
            nameof(ScaleLo),
            typeof(float),
            typeof(Mandelbrot),
            new UIPropertyMetadata(
                0.0f,
                PixelShaderConstantCallback(3)));

    public float Aspect
    {
        get => (float)GetValue(AspectProperty);
        set => SetValue(AspectProperty, value);
    }

    public static readonly DependencyProperty AspectProperty =
        DependencyProperty.Register(
            nameof(Aspect),
            typeof(float),
            typeof(Mandelbrot),
            new UIPropertyMetadata(
                1.0f,
                PixelShaderConstantCallback(4)));

    public Color BackgroundColor
    {
        get => (Color)GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    public static readonly DependencyProperty BackgroundColorProperty =
        DependencyProperty.Register(
            nameof(BackgroundColor),
            typeof(Color),
            typeof(Mandelbrot),
            new UIPropertyMetadata(
                Colors.Black,
                PixelShaderConstantCallback(5)));

    public Color EscapeColor
    {
        get => (Color)GetValue(EscapeColorProperty);
        set => SetValue(EscapeColorProperty, value);
    }

    public static readonly DependencyProperty EscapeColorProperty =
        DependencyProperty.Register(
            nameof(EscapeColor),
            typeof(Color),
            typeof(Mandelbrot),
            new UIPropertyMetadata(
                Colors.White,
                PixelShaderConstantCallback(6)));

    public Color SetColor
    {
        get => (Color)GetValue(SetColorProperty);
        set => SetValue(SetColorProperty, value);
    }

    public static readonly DependencyProperty SetColorProperty =
        DependencyProperty.Register(
            nameof(SetColor),
            typeof(Color),
            typeof(Mandelbrot),
            new UIPropertyMetadata(
                Colors.Black,
                PixelShaderConstantCallback(7)));
}
