# Tutorial 2

## Goals
* Understanding how geometry is pumped through the rendering pipeline.

* Understaning `unifrom`, `attribute` and `varying` shader variables.

* Understand how data is passed from pixel to vertex shader.

* Grasp basics of 3D transformation.

## Prerequisites
* Make sure you got [Tutorial 1](../Tutorial_1) up and running.

## Passing more information through the pipeline
First, let's add some more triangles to the geometry. Add one more vertex and span four triangles with the four vertices to create a Tetrahedron ("a triangular pyramid"). Extend the Mesh instantiation in `Tutorial_2` to the following (you may omit the comments):

```csharp
_mesh = new Mesh
{
    Vertices = new[]
    {
        new float3(-0.8165f, -0.3333f, -0.4714f),   //Vertex 0
        new float3(0.8165f, -0.3333f, -0.4714f),    //Vertex 1
        new float3(0, -0.3333f, 0.9428f),           //Vertex 2
        new float3(0, 1, 0),                        //Vertex 3
    },
    Triangles = new ushort[]
    {
        0, 2, 1,    //Triangle 0 "Bottom" facing towards negative  axis.
        0, 1, 3,    //Triangle 1 "Back side" facing towards negative z axis.
        1, 2, 3,    //Triangle 2 "Right side" facing towards positive x axis.
        2, 0, 3,    //Triangle 3 "Left side" facing towards negative x axis.
    },
};
```
Debuggin the program in this way will stil result in a single triangle being displayed.

![First Triangle](_images/FirstTriangle.png)

This is Triangle 1, the "back side" of the tetrahedon. The other three triangles are obscured by the one we are seeing. Note that the visible triangle is somewhat out of center towards the upper border of the window. This is because the vertex coordinates used above are chosen to make their common origin (0, 0, 0) to be the tetrahedron's center of gravity.

## Rotating it
Now we would like to rotate the tetrahedron. There are two ways to accomplish this:

1. We could change the Mesh#s vertex coordinates every frame within `RenderAFrame`.

2. We could transform the Mesh's vertex coordinates every frame from within the vertex shader.

The typical way to perform coordinate transformations is option 2, especially if we are performing linear transformations (such as a rotation in our case). Let's recall some maths to see how an arbitrary 2D vector (x, y) is rotated around an angle alpha to yield the new coordinates (x', y'):

```
x' = x * cos(alpha) + y * -sin(alpha)
y' = x * sin(alpha) + y *  cos(alpha)
```

From linear algebra you might remember that his can as well be written in matrix format. We will revisit matrices later on. Right now we want to apply the above directly to our vertex shader:

```glsl
attribute vec3 fuVertex;

void main()
{
    float alpha = 3.1415 / 4; //(45 degrees)
    float s = sin(alpha);
    float c = cos(alpha);
    gl_Position = vec4( fuVertex.x * c - fuVertex.y * s,    //The transformed x coordinate
                        fuVertex.x * s + fuvertex.y * c,    //The transformed y coordinate
                        fuVertex.z,                         //z is unchanged
                        1.0);
}
```

Debugging the program should result in the triangle rotated about 45 degrees counterclockwise.

![First Triangle](_images/SecondTriangle.png)

### Practice
* Try other amounts for alpha to see how the rotation behaves.

* Go 3D! Rotate around the y-axis instead of the z-axis. That is, leave the `fuVertex.y` unchanged and apply the sin/cos factors to x and z. This way you will get to see the other sides of the tetrahedon. Unfortunately you cannot tell the border between the different sides because they are all the same color. Instead, you will always see a triangular silhuette.

## Animation
Notice that we now have a single parameter (`alpha`) controlling the transformation of all vertices within our mesh. This parameter is set to a constant value in the vertex shader. If we could find a way to alter the value of `alpha` from one frame to the otherm we could implement a rotation animation. Shader languages allow to set individual values from the "outside world" through so called "uniform variables". Let's change `alpha` from a constant local variable inside the vertex shader's `main` function to a more "global" uniform variable. Change the first lines of the vertex shader like this:

```glsl
attribute vec3 fuVertex;
uniform float alpha;

void main()
{
    float s = sin(alpha);
    ...
```

`alpha` now looks like a global variable (outside of `main`). In addition it is decorated with the `uniform` keyword, which marks it as being a value that changes rather infrequently (the vertex shader will be called for a lot of vertices while `alpha`'s value doesn#t change). This is in contrast to the `fuVertex` variable on the line above, which contains a different value (the vertex itself) for each time the vertex shader is called. Thus, this variable is marked being an `attribute` (and NOT a `uniform`).

Before we can change the value of `alpha` (which - as part of the vertex shader - lives on the GPU) from the application code (which runs on the CPU), we need to have an identifier to access our variable. First, we need to declare one field within our `Tutorial_2` class:

```csharp
private float _alpha
```

The field `_alpha` will keep the actual value of the GPU-variable `alpha` in CPU-Land.

Inside the `Init` method, we will now initialize `_alpha` and make a new `EffectParameterDeclaration` for `alpha`.

```csharp
_alpha = 0;

_shaderEffect = new ShaderEffect(
    new[]
    {
        new EffectPassDeclaration{VS = _vertexShader, PS = _pixelShader, StateSet = new RenderStateSet{}}
    },
    new[]
    {
        new EffectParameterDeclaration { Name = "DiffuseColor", Value = new float4(1, 0, 1, 1) },
        new EffectParameterDeclaration { Name = "alpha", Value = _alpha }
    }
);
```

Then, in the `RenderAFrame` method, we can alter the contents of `_alpha` and then pass the new value up to the GPU's `alpha` variable:

```csharp
_alpha += 0.01f;
_shaderEffect.SetEffectParam("alpha", _alpha)
```

This way, each frame the angle `alpha` will be incremented about 0.01 radians.

Debugging this will show a somewhat rotating triangle. If you rotate around the y-axis as proposed in the previous paragraph, you will rather recognize a triangle bouncing back and forth. Remember that you are really seeing the triangular silhuette of rotating threedimensional tetrahedron.

### Practice
* Create a uniform variable in the pixel shader and do some color animation.