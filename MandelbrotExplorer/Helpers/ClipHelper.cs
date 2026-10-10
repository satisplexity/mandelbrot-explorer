using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace MandelbrotExplorer.Helpers;

public static class ClipHelper
{
    public static readonly DependencyProperty EnableProperty
        = DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(ClipHelper),
            new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty RadiusProperty
        = DependencyProperty.RegisterAttached(
            "Radius",
            typeof(CornerRadius),
            typeof(ClipHelper),
            new PropertyMetadata(default(CornerRadius), OnRadiusChanged));

    public static void SetEnable(DependencyObject element, bool value)
        => element.SetValue(EnableProperty, value);

    public static bool GetEnable(DependencyObject element)
        => (bool)element.GetValue(EnableProperty);

    public static void SetRadius(DependencyObject element, CornerRadius value)
        => element.SetValue(RadiusProperty, value);

    public static CornerRadius GetRadius(DependencyObject element)
        => (CornerRadius)element.GetValue(RadiusProperty);

    private static void OnEnableChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        if (sender is not FrameworkElement element)
            return;

        if ((bool)args.NewValue)
        {
            element.SizeChanged += Element_SizeChanged;
            element.Loaded += Element_Loaded;

            UpdateClip(element);
        }
        else
        {
            element.SizeChanged -= Element_SizeChanged;
            element.Loaded -= Element_Loaded;

            element.Clip = null;
        }
    }

    private static void OnRadiusChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        if (sender is FrameworkElement element && GetEnable(element))
            UpdateClip(element);
    }

    private static void Element_Loaded(object sender, RoutedEventArgs args)
        => UpdateClip((FrameworkElement)sender);

    private static void Element_SizeChanged(object sender, SizeChangedEventArgs args)
        => UpdateClip((FrameworkElement)sender);

    private static void UpdateClip(FrameworkElement element)
    {
        double width = element.ActualWidth;
        double height = element.ActualHeight;

        if (width <= 0 || height <= 0)
        {
            element.Clip = null;

            return;
        }

        CornerRadius radius = GetEffectiveRadius(element);

        double maxRadius = Math.Min(width, height) / 2;

        radius.TopLeft = Math.Min(radius.TopLeft, maxRadius);
        radius.TopRight = Math.Min(radius.TopRight, maxRadius);
        radius.BottomLeft = Math.Min(radius.BottomLeft, maxRadius);
        radius.BottomRight = Math.Min(radius.BottomRight, maxRadius);

        element.Clip = CreateGeometry(width, height, radius);
    }

    private static CornerRadius GetEffectiveRadius(FrameworkElement element)
    {
        if (element is Border border)
            return border.CornerRadius;

        return GetRadius(element);
    }

    private static StreamGeometry CreateGeometry(double width, double height, CornerRadius radius)
    {
        StreamGeometry geometry = new();

        using StreamGeometryContext ctx = geometry.Open();

        ctx.BeginFigure(
            new Point(radius.TopLeft, 0),
            true,
            true);

        ctx.LineTo(
            new Point(width - radius.TopRight, 0),
            true,
            false);

        ctx.ArcTo(
            new Point(width, radius.TopRight),
            new Size(radius.TopRight, radius.TopRight),
            0,
            false,
            SweepDirection.Clockwise,
            true,
            false);

        ctx.LineTo(
            new Point(width, height - radius.BottomRight),
            true,
            false);

        ctx.ArcTo(
            new Point(width - radius.BottomRight, height),
            new Size(radius.BottomRight, radius.BottomRight),
            0,
            false,
            SweepDirection.Clockwise,
            true,
            false);

        ctx.LineTo(
            new Point(radius.BottomLeft, height),
            true,
            false);

        ctx.ArcTo(
            new Point(0, height - radius.BottomLeft),
            new Size(radius.BottomLeft, radius.BottomLeft),
            0,
            false,
            SweepDirection.Clockwise,
            true,
            false);

        ctx.LineTo(
            new Point(0, radius.TopLeft),
            true,
            false);

        ctx.ArcTo(
            new Point(radius.TopLeft, 0),
            new Size(radius.TopLeft, radius.TopLeft),
            0,
            false,
            SweepDirection.Clockwise,
            true,
            false);

        geometry.Freeze();

        return geometry;
    }
}