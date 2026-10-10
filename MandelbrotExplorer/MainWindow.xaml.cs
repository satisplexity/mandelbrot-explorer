using MandelbrotExplorer.Effects;
using MandelbrotExplorer.Structs;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows;

namespace MandelbrotExplorer;

/// <summary>
/// Displays the fractal and handles smooth camera zooming and panning.
/// </summary>
public partial class MainWindow : Window
{
    // Controls how quickly the camera approaches its target state.
    // Higher values produce a faster response.
    private const double SMOOTH_SPEED = 12.0;

    // Target and current horizontal positions in fractal coordinates.
    private DoubleDouble _targetCenterX = DoubleDouble.FromDouble(-0.5);
    private DoubleDouble _centerX = DoubleDouble.FromDouble(-0.5);

    // Target and current vertical positions in fractal coordinates.
    private DoubleDouble _targetCenterY = DoubleDouble.FromDouble(0.0);
    private DoubleDouble _centerY = DoubleDouble.FromDouble(0.0);

    // Target and current scales. Smaller values zoom further into the fractal.
    private DoubleDouble _targetScale = DoubleDouble.FromDouble(1.5);
    private DoubleDouble _scale = DoubleDouble.FromDouble(1.5);

    // Tracks whether a mouse drag is currently moving the camera.
    private bool _isPanning;

    // Mouse position recorded during the previous drag event.
    private Point _lastMousePosition;

    // Measures elapsed time for frame-rate-independent smoothing.
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private double _lastTime;

    /// <summary>
    /// Initializes the window and subscribes to mouse and rendering events.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        Fractal.MouseLeftButtonDown += Fractal_OnMouseLeftButtonDown;
        Fractal.MouseLeftButtonUp += Fractal_OnMouseLeftButtonUp;
        Fractal.MouseWheel += Fractal_OnMouseWheel;
        Fractal.MouseMove += Fractal_OnMouseMove;

        CompositionTarget.Rendering += OnRendering;
    }

    /// <summary>
    /// Advances the camera toward its target state and updates the display
    /// before rendering.
    /// </summary>
    private void OnRendering(object? sender, EventArgs e)
    {
        double currentTime = _clock.Elapsed.TotalSeconds;

        // Calculate the elapsed time in seconds since the previous update.
        double delta = currentTime - _lastTime;
        _lastTime = currentTime;

        // Convert the elapsed time into an exponential smoothing factor.
        double factor = 1.0 - Math.Exp(-SMOOTH_SPEED * delta);

        _scale = Smooth(_scale, _targetScale, factor);
        _centerX = Smooth(_centerX, _targetCenterX, factor);
        _centerY = Smooth(_centerY, _targetCenterY, factor);

        UploadCamera();
        UpdateZoomText();
    }

    /// <summary>
    /// Displays the current zoom relative to the initial scale of 1.5.
    /// </summary>
    private void UpdateZoomText()
    {
        // Approximate the scale as a double for display purposes.
        double dispScale = _scale.High + _scale.Low;

        ZoomText.Text = $"Zoom {Math.Round(1.5 / dispScale, 1)}x";
    }

    /// <summary>
    /// Moves the current value toward the target by the specified factor.
    /// Snaps to the target when the absolute difference is sufficiently small.
    /// </summary>
    private static DoubleDouble Smooth(DoubleDouble current, DoubleDouble target, double factor)
    {
        DoubleDouble delta = target - current;

        // This fixed threshold may bypass smoothing at very deep zoom levels.
        if (Math.Abs(delta.High + delta.Low) <= 1e-12)
            return target;

        // Keep the interpolation factor within the valid range.
        return current + delta * Math.Clamp(factor, 0.0, 1.0);
    }

    /// <summary>
    /// Starts panning and captures the mouse to continue receiving drag events
    /// outside the fractal area.
    /// </summary>
    private void Fractal_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouse)
    {
        _isPanning = true;
        _lastMousePosition = mouse.GetPosition(Fractal);

        Fractal.CaptureMouse();

        mouse.Handled = true;
    }

    /// <summary>
    /// Updates the target camera position according to the mouse drag distance.
    /// </summary>
    private void Fractal_OnMouseMove(object sender, MouseEventArgs mouse)
    {
        if (!_isPanning)
            return;

        double width = Fractal.ActualWidth;
        double height = Fractal.ActualHeight;

        if (width <= 0 || height <= 0)
            return;

        Point currentPosition = mouse.GetPosition(Fractal);

        // Measure mouse movement in local WPF coordinates.
        double mouseDeltaX = currentPosition.X - _lastMousePosition.X;
        double mouseDeltaY = currentPosition.Y - _lastMousePosition.Y;

        _lastMousePosition = currentPosition;

        double aspect = width / height;

        // Convert the drag distance into normalized viewport units,
        // accounting for the viewport'scale aspect ratio.
        double deltaX = mouseDeltaX * 2.0 / width * aspect;
        double deltaY = mouseDeltaY * 2.0 / height;

        // Translate the target center so the fractal follows the mouse.
        // Screen Y increases downward, while fractal Y increases upward.
        _targetCenterX = _targetCenterX - _targetScale * deltaX;
        _targetCenterY = _targetCenterY + _targetScale * deltaY;

        mouse.Handled = true;
    }

    /// <summary>
    /// Stops panning and releases mouse capture.
    /// </summary>
    private void Fractal_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs mouse)
    {
        _isPanning = false;

        Fractal.ReleaseMouseCapture();

        mouse.Handled = true;
    }

    /// <summary>
    /// Changes the target zoom while preserving the fractal point
    /// under the mouse in the target camera state.
    /// </summary>
    private void Fractal_OnMouseWheel(object sender, MouseWheelEventArgs mouse)
    {
        double width = Fractal.ActualWidth;
        double height = Fractal.ActualHeight;

        if (width <= 0 || height <= 0)
            return;

        Point position = mouse.GetPosition(Fractal);

        // Map the mouse position to the range [-1, 1] across the viewport.
        // Invert Y to match the fractal coordinate system.
        double nx = 2.0 * position.X / width - 1.0;
        double ny = 1.0 - 2.0 * position.Y / height;

        double aspect = width / height;

        // Find the fractal point under the mouse using the target camera.
        DoubleDouble anchorX = _targetCenterX + _targetScale * (nx * aspect);
        DoubleDouble anchorY = _targetCenterY + _targetScale * ny;

        // A positive wheel delta reduces the scale and zooms in.
        // A delta of 120 corresponds to one standard wheel notch.
        double factor = Math.Pow(0.85, mouse.Delta / 120.0);

        DoubleDouble newScale = _targetScale * factor;

        // Adjust the target center to keep the anchor at the same
        // viewport position after changing the target scale.
        _targetCenterX = anchorX - newScale * (nx * aspect);
        _targetCenterY = anchorY - newScale * ny;

        _targetScale = newScale;
    }

    /// <summary>
    /// Sends the current camera position, scale, and aspect ratio to the shader.
    /// </summary>
    private void UploadCamera()
    {
        if (Fractal.ActualWidth <= 0 || Fractal.ActualHeight <= 0)
            return;

        // Convert each camera value into leading and residual float components.
        var centerX = _centerX.ToShaderPair();
        var centerY = _centerY.ToShaderPair();
        var scale = _scale.ToShaderPair();

        Mandelbrot.CenterHi = new Point(centerX.High, centerY.High);
        Mandelbrot.CenterLo = new Point(centerX.Low, centerY.Low);

        Mandelbrot.ScaleHi = scale.High;
        Mandelbrot.ScaleLo = scale.Low;

        // Supply the viewport aspect ratio for coordinate mapping.
        Mandelbrot.Aspect = (float)(Fractal.ActualWidth / Fractal.ActualHeight);
    }

    private void Slider_ValueChanded(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        int maxIterations = (int)MaxIterationsSlider.Value;

        if (MaxIterationText is not null)
            MaxIterationText.Text = maxIterations.ToString();

        if(Mandelbrot is not null)
            Mandelbrot.IterationsLimit = maxIterations;
    }
}