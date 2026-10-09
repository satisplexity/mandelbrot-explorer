using MandelbrotExplorer.Effects;
using MandelbrotExplorer.Structs;
using System.Windows;
using System.Windows.Input;

namespace MandelbrotExplorer;

public partial class MainWindow : Window
{
    private bool _isPanning;
    private Point _lastMousePosition;

    private DoubleDouble _centerX = DoubleDouble.FromDouble(-0.5);

    private DoubleDouble _centerY = DoubleDouble.FromDouble(0.0);

    private DoubleDouble _scale = DoubleDouble.FromDouble(1.5);

    public MainWindow()
    {
        InitializeComponent();

        Loaded += (_, _) => UploadCamera();

        Fractal.MouseLeftButtonDown += Fractal_OnMouseLeftButtonDown;
        Fractal.MouseLeftButtonUp += Fractal_OnMouseLeftButtonUp;
        Fractal.MouseMove += Fractal_OnMouseMove;
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

        _centerX = _centerX - _scale * deltaX;

        _centerY = _centerY + _scale * deltaY;

        UploadCamera();

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


        DoubleDouble anchorX = _centerX + _scale * (nx * aspect);
        DoubleDouble anchorY = _centerY + _scale * ny;

        double factor = Math.Pow(0.85, e.Delta / 120.0);

        DoubleDouble newScale = _scale * factor;


        _centerX = anchorX - newScale * (nx * aspect);

        _centerY = anchorY - newScale * ny;

        _scale = newScale;

        double dispScale = _scale.Hi + _scale.Lo;

        ZoomText.Text = $"Zoom {Math.Round(1.5 / dispScale, 1)}x";

        UploadCamera();
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

    private void Fractal_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UploadCamera();
    }
}