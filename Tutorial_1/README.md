# Tutorial 1

## Goals
* Understand the basic setup of a FUSEE application.
* Understand rendering pipeline basics.
* Send simple geometry through the rendering pipeline.

## Getting Started
* Install and run FUSEE and its dependencies as explained on the [FUSEE Website](https://www.fusee3d.org).

* Download or clone this repository to your computer and open this folder (`Tutorial_1`) in Visual Studio Code.

* Go to the Debug view of Visual Studio Code by selecting it in the Activity Bar on the side (or by pressing `Ctrl+Shift+D`).

* At the top, open the dropdown menu and select either `Debug in FUSEE Player` or `Debug in FUSEE Web Player`. If you click `Start Debugging` (the play icon), you should see a window with a simple white background.

* In general, FUSEE applications can be built for both Desktop or Web environments. To do so, use the console command `"fusee publish --platform Web"` or `"fusee publish --platform Desktop"`.

    * You can find the published files in `/pub/Desktop` or `/pub/Web` respectively.

        *Note: Add picture of folder structure.*

## Basic Structure
* This folder contains one class, `Tutorial_1.cs`. This contains the application logic, right now consisting of three methods:

    * `Init` - Called once on application startup. You can place initialization code here. Currently, `Init` only sets the clear color

        ```csharp
        public override void Init()
        {
            RC.ClearColor = new float4(1, 1, 1, 1);
        }
        ```

    * `RenderAFrame` - Called to generate image contents (once per frame). You can place drawing and interaction code here. Currently, `RenderAFrame` just clears the background of the backbuffer and copies the backbuffer contents to the front buffer.

        ```csharp
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            // Copy backbuffer contents to the front buffer.
            Present();
        }
        ```
    
    * `Resize` - Called when the render window is resized (or initialized). We will oook at this method later.

* Try to change the color of the render window background by altering the first three components of the `float4` value assigned to `RC.ClearColor`. These values are red,green, and blue intensities in the range from 0 to 1.

## The Rendering Pipeline
Before we can draw some geometry, we need a simple understanding how the graphics card works. You can imagine the graphics card (GPU) as a pipeline. On the one end, you put in (3D-)Geometry and on the other end a rendered two-dimensional pixel image drops out. The conversion from the vector geometry into pixel images is done in two major steps:

1. The coordinates of the geometry's vertices are converted into screen coordinates.

2. The vector geometry (in screen coordinates) is "rasterized" - that is, each pixel in the output buffer covered by geometry is filled with a certain color.

You can control both steps by placing small programs on the GPU's processor. These programs are called "Shaders". A program performing the coordinate transformation fom whatever source-coordinate system to screen coordinates is called "Vertex Shader". A program performing the color calculation of each pixel to fill is called "Pixel Shader". In FUSEE you need to provide a Pixel and a Vertex Shader if you want to render geometry. The programming language for shaders used in FUSEE is GLSL, the shader language supported by OpenGL.

![Render Pipeline](_images/RenderPipelineVP.png)

