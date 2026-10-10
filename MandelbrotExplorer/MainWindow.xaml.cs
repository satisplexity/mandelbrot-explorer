using MandelbrotExplorer.Effects;
using MandelbrotExplorer.Structs;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows;

namespace MandelbrotExplorer;

public partial class MainWindow : Window
{
    private const double SMOOTH_SPEED = 12.0;
    
    private DoubleDouble _targetCenterX = DoubleDouble.FromDouble(-0.5);
    private DoubleDouble _centerX = DoubleDouble.FromDouble(-0.5);
    
    private DoubleDouble _targetCenterY = DoubleDouble.FromDouble(0.0);
    private DoubleDouble _centerY = DoubleDouble.FromDouble(0.0);
    
    private DoubleDouble _targetScale = DoubleDouble.FromDouble(1.5);
    private DoubleDouble _scale = DoubleDouble.FromDouble(1.5);
    
    private bool _isPanning;
    private Point _lastMousePosition;

    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private double _lastTime;  

    public MainWindow()
    {
        InitializeComponent();

        Fractal.MouseLeftButtonDown += Fractal_OnMouseLeftButtonDown;
        Fractal.MouseLeftButtonUp += Fractal_OnMouseLeftButtonUp;
        Fractal.MouseWheel += Fractal_OnMouseWheel;
        Fractal.MouseMove += Fractal_OnMouseMove;

        CompositionTarget.Rendering += OnRendering;
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        double currentTime = _clock.Elapsed.TotalSeconds;

        double delta = currentTime - _lastTime;
        _lastTime = currentTime;

        double factor = 1.0 - Math.Exp(-SMOOTH_SPEED * delta);

        _scale = Smooth(_scale, _targetScale, factor);
        _centerX = Smooth(_centerX, _targetCenterX, factor);
        _centerY = Smooth(_centerY, _targetCenterY, factor);

        UploadCamera();
        UpdateZoomText();
    }

    private void UpdateZoomText()
    {
        double dispScale = _scale.Hi + _scale.Lo;

        ZoomText.Text = $"Zoom {Math.Round(1.5 / dispScale, 1)}x";
    }

    private static DoubleDouble Smooth(DoubleDouble current, DoubleDouble target, double factor)
    {
        DoubleDouble delta = target - current;

        if (Math.Abs(delta.Hi + delta.Lo) <= 1e-12)
            return target;

        return current + delta * Math.Clamp(factor, 0.0, 1.0);
    }

    private void Fractal_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isPanning = true;
        _lastMousePosition = e.GetPosition(Fractal);

        Fractal.CaptureMouse();

        e.Handled = true;
    }

    private void Fractal_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isPanning)
            return;

        double width = Fractal.ActualWidth;
        double height = Fractal.ActualHeight;

        if (width <= 0 || height <= 0)
            return;

        Point currentPosition = e.GetPosition(Fractal);

        double dx = currentPosition.X - _lastMousePosition.X;
        double dy = currentPosition.Y - _lastMousePosition.Y;

        _lastMousePosition = currentPosition;

        double aspect = width / height;

        double deltaX = dx * 2.0 / width * aspect;
        double deltaY = dy * 2.0 / height;

        _targetCenterX = _targetCenterX - _targetScale * deltaX;

        _targetCenterY = _targetCenterY + _targetScale * deltaY;

        e.Handled = true;
    }

    private void Fractal_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isPanning = false;

        Fractal.ReleaseMouseCapture();

        e.Handled = true;
    }

    private void Fractal_OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        double width = Fractal.ActualWidth;
        double height = Fractal.ActualHeight;

        if (width <= 0 || height <= 0)
            return;

        Point mouse = e.GetPosition(Fractal);

        double nx = 2.0 * mouse.X / width - 1.0;
        double ny = 1.0 - 2.0 * mouse.Y / height;

        double aspect = width / height;

        DoubleDouble anchorX = _targetCenterX + _targetScale * (nx * aspect);
        DoubleDouble anchorY = _targetCenterY + _targetScale * ny;

        double factor = Math.Pow(0.85, e.Delta / 120.0);

        DoubleDouble newScale = _targetScale * factor;

        _targetCenterX = anchorX - newScale * (nx * aspect);
        _targetCenterY = anchorY - newScale * ny;

        _targetScale = newScale;
    }

    private void UploadCamera()
    {
        if (Fractal.ActualWidth <= 0 || Fractal.ActualHeight <= 0)
            return;

        var x = _centerX.ToShaderPair();
        var y = _centerY.ToShaderPair();
        var s = _scale.ToShaderPair();

        Mandelbrot.CenterHi = new Point(x.Hi, y.Hi);

        Mandelbrot.CenterLo = new Point(x.Lo, y.Lo);

        Mandelbrot.ScaleHi = s.Hi;
        Mandelbrot.ScaleLo = s.Lo;

        Mandelbrot.Aspect = (float)(Fractal.ActualWidth / Fractal.ActualHeight);
    }
}