# Mandelbrot Explorer

An interactive Mandelbrot set explorer built with **C#, WPF, and HLSL**. Navigate the complex plane with smooth zooming and panning, and explore fractal detail through a custom pixel shader.

## Screenshots

<!-- Add your images to docs/screenshots/ and keep the filenames below,
     or update the paths to match your files. -->

### Overview

<p align="center">
  <img src="https://raw.githubusercontent.com/satisplexity/mandelbrot-explorer/main/docs/screentshots/overview.png"
       alt="Overview">
</p>

### Zoomed view

<p align="center">
  <img src="https://raw.githubusercontent.com/satisplexity/mandelbrot-explorer/main/docs/screentshots/zoom.png"
       alt="Zoom">
</p>

### Fractal detail

<p align="center">
  <img src="https://raw.githubusercontent.com/satisplexity/mandelbrot-explorer/main/docs/screentshots/detail.png"
       alt="Detail">
</p>

## Features

- **Shader-based rendering** — generates the fractal procedurally using a WPF ShaderEffect.
- **Smooth navigation** — interpolates the camera position and scale over time.
- **Mouse-centered zoom** — adjusts the target camera around the mouse position.
- **Drag-to-pan controls** — moves through the complex plane with the left mouse button.
- **Adjustable iteration limit** — controls how many iterations are evaluated for each point.
- **Extended-precision arithmetic** — uses paired numeric components for camera values and shader calculations.
- **Smooth escape-time coloring** — reduces discrete color bands with a logarithmic correction.
- **Configurable shader colors** — exposes background, escape, and set colors through dependency properties.
- **Zoom indicator** — displays magnification relative to the initial view.

## Controls

| Action | Control |
| --- | --- |
| Zoom in or out | Mouse wheel |
| Pan | Hold the left mouse button and drag |
| Change the iteration limit | Max iterations slider |

## How it works

Each pixel is mapped to a point `c` in the complex plane. Starting with `z = 0`, the shader repeatedly evaluates:

```text
z = z² + c
```

If the magnitude of `z` exceeds 2, the point is colored according to its escape time. Points that do not escape within the iteration limit use the set color.

Camera values are maintained on the CPU as pairs of `double` values. Before rendering, they are converted to pairs of `float` values for the shader. Custom addition and multiplication routines retain a low-order correction during GPU calculations.

This extends precision beyond ordinary single-float arithmetic, but does not preserve the full precision of the CPU representation. Zoom depth remains limited by shader precision and accumulated numerical error.

## Build and run

1. Open the solution in Visual Studio on Windows with the **.NET desktop development** workload installed.
2. Install the .NET SDK targeted by the project if it is not already available.
3. Restore dependencies, build the solution, and run the application.

### Compiling the shader

If you modify the HLSL source, compile it with the Windows SDK's `fxc.exe` and replace the compiled `Shaders/Mandelbrot.ps` resource before rebuilding.

Run the following from the directory containing `Mandelbrot.hlsl`, with `fxc` available on your PATH:

```bat
fxc /T ps_3_0 /E main /Gis /Fo Mandelbrot.ps Mandelbrot.hlsl
```

The `/Gis` option requests IEEE-strict compilation, which matters for arithmetic routines that recover rounding residuals.

## Notes

- Increasing the iteration limit can reveal points that escape later, at a higher rendering cost.
- More iterations do not increase coordinate precision or remove artifacts caused by its limits.
- Exterior colors are currently normalized by the iteration limit, so changing that limit also changes the gradient.
- A point that does not escape within a finite iteration budget is not necessarily a confirmed member of the Mandelbrot set.

