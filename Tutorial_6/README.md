# Tutorial 6

## Goals
* See how textures can be made available in the pixel shader.
* Understand texture coordinates as part of the geometry.
* Understand the interplay between shaders, multiple render passes, and render states.

## Welcome to WuggyLand
Open Tutorial_6 in Visual Studio Code and debug it. A little interactive scene has been created around the Wuggy rover: Using the `W`, `A`, `S`, and `D` keys, you can move the rover around the ground plane.

![wuggyLand](_images\WuggyLand.png)

The entire scne is now contained in a single `.fus` file. All parts of the scene that need to be altered at run-time are identified by unique object names.

But let's look at the things that additionally changed under the hood.

* The [Pixel Shader](Assets\PixelShader.frag) now contains a more sophisticated specular color handling.
  ```glsl
  float intensitySpec = 0.0;
    if (intensityDiff > 0.0) {
      vec3 viewdir = -viewpos;
      vec3 h = normalize(viewdir + lightdir);
      intensitySpec = specfactor * pow(max(0.0, dot(h, nnormal)), shininess);
    }

    gl_FragColor = vec4(intensityDiff * albedo + intensitySpec * speccolor, 1);
  ```

* These new parameters are set from respective entries in the various shader effect components in the `WuggyLand.fus` file. This happens in the now extended OnShaderEffect() method found in `Tutorial.cs`

### Practice
* Take a look at the new Pixel Shader and try to figure out what the new `uniform` parameters do. Compare the changes to the Pixel Shader from the previous tutorial. Temporarily comment out parts of the final color calculation to see their individual contribution.

* Set a breakpoint within `OnShaderEffect()` and step through the various materials in `WuggyLand.fus`. Watch the `CurrenNode.Name` property to identify which shader effect is used on which object.

* Watche the contents of the shader effect component currently visited. What other information is contained here which is currently not handled?

## Adding texture information
In `WuggyLand.fus`, the green parts of the tree models are already prepared to show a very simple leaf-like structure by displaying an image spread over the rounded shapes. If you performed the last point of the Practice block above, you might have noticed that several shader effect components contain additional parameters with the name of "DiffuseTexture" and "Texmix".

![parameters](_images\parameters.png)

This string property contains a Texture object associated with the `Leaves.jpg` image. You can take a look at its location in the `Assets` folder.

![leaves](/Assets/Leaves.jpg)

Now we want to disply this image on the green round treetop models. To do this, we have to accomplish two things:

1. Allow the Pixel Shader to access the pixels inside `Leaves.jpg`.
2. Tell the Pixel shader for each screen pixel (a.k.a. Fragment) it is about to render, which pixel from the texture (a.k.a. Texel) it should take as the `albedo`.

## Textures are `uniform` Shader Parameters
Everything that controlled the process how a vertex shader processes coordinates or how a pixel shader calculates the color for a given screen pixel was passed into the shader as a `uniform` parameter. We have seen single `float` values, `float3` values (used as colors), and `float4x4` matrix values.

Since a texture is also quite something that influences the way an output color should be calculated, it is a `uniform` parameter as well. Because there is much more data behind such a `uniform` parameter than in the cases before, there are some things that are different compared to "ordinary" `uniform` parameters:

* We want to be able to read the contents of a texture image from a file.
* We want to be able to upload the texture contents to the GPU memory and "address" it somehwo when needed rather than uploading all the pixels contained in a texture every frame.

FUSEE has some functionality we can use to do this. Perform the following steps:

* First of all, add `Leaves.jpg` to the Assets folder if it isn't there already.

* In the constructor of our `Renderer` class, get the asset's contents as an instance of the ImageData structure and turn it into a texture.
  ```csharp
  var leaves = AssetStorage.Get<ImageData>("Leaves.jpg");
  _leavesTexture = new Texture(leaves);
  ```

* Now add new parameters to the EffectParameterDeclaration. *(Note: We do not set the texture, yet, since we don't want it applied to all nodes.)*
  ```csharp
  new EffectParameterDeclaration { Name = "texmix", Value = 0.0f }
  ```
  And don't forget to add `leavesTexture` as a field at the class level of the `Renderer` class.

* To be able to access the texture in the pixel shader, add two `uniform` parameters to `PixelShader.frag`.
  ```glsl
  uniform sampler2D texture;
  uniform float texmix;
  ```
  Note the datatype `sampler2D` in comparison to the datatypes we already used for `uniform` parameters.

* Now we want to read a color value out of the `texture`. This can be done using the `texture2D()` function declared in GLSL. The first parameter of `texture2D()` is the texture to read from. The second parameter is a 2D coordinate where both dimensions may contain values from 0 to 1. We will simply pass (0, 0) for now, denoting the pixel in the lower left corner of the image. In addition, we will use the `texmix` variable as a means to mix the color value passed in albedo with the color read from the texture. All in all, the resulting pixel shader should look like this:
  ```glsl
  #ifdef GL_ES
  precision highp float;
  #endif

  varying vec3 viewpos;
  varying vec3 normal;
  varying vec2 uv;
  uniform vec3 albedo;
  uniform float shininess;
  uniform float specfactor;
  uniform vec3 speccolor;
  uniform sampler2D texture;
  uniform float texmix;

  void main() {
    vec3 nnormal = normalize(normal);

    // Diffuse
    vec3 lightdir = vec3(0, 0, -1);
    float intensityDiff = dot(nnormal, lightdir);
    vec3 resultingAlbedo = (1.0 - texmix) * albedo + texmix * vec3(texture2D(texture, uv));

    // Specular
    float intensitySpec = 0.0;
    if (intensityDiff > 0.0) {
      vec3 viewdir = -viewpos;
      vec3 h = normalize(viewdir + lightdir);
      intensitySpec = specfactor * pow(max(0.0, dot(h, nnormal)), shininess);
    }

    gl_FragColor = vec4(intensityDiff * resultingAlbedo + intensitySpec * speccolor, 1);
  }
  ```
  Note how the `resultingAlbedo` is now calculated as a mixture between the original `albedo` and the color at (0, 0) in `texture`.

* Finally, in `OnShaderEffect` check if we are at the right node and set our `uniform` to be the leaves image, as well as `texmix` to 0 or 1, depending on the presence of a texture.
  ```csharp
  if (CurrentNode.Name.Contains("Kugel"))
  {
      RC.SetFXParam("texture", _leavesTexture);
      RC.SetFXParam("texmix", 1.0f);
  }
  else
  {
      RC.SetFXParam("texmix", 0.0f);
  }
  ```

Debugging the program should result in no changes as the corner we are taking the color information from is nearly the same as the overall diffuse color of the treetop objects.

### Practice
* Create a spare copy `Leaves.jpg`, open the original in an image editing software and assign a color other than green to the lower left corner of the image. Save the image and run the application again to see the treetops appear entirely in your chosen color.
* Just to use other texture coordinates than the original: use the normalized normal's x and y as texture coordinates and try to explain what you see as a result.

## Texture Coordinates
Instead of constantly reading the lower left pixel out of our image we now want to read out the correct pixel from the image texture. Typically, the information how a texture is applied to an object is stored with the obect's vertices in so called ***Texture Coordinates***. Every vertex contains a two-dimensional set of texture coordinates (mostly in the range of 0 to 1). So the model itself contains this mapping information (often called uv-coordinates, or UVs). The following image shows how a vertex has a set of UV coordinates attached (0.5, 0.5) and how this value identifies a pixel position in the texture.

![UV cordinates](_images\SingleVertexUv.png)

Once a triangle is rendered, a texture coordinate for every pixel of the triangle can be interpolated from the texture coordinates at the three vertices of the triangle based on the relative posiiton of the pixel to the triangle's vertices. This interpolation happens exactly in the same way we are interpolating normals: As part of the interpolation functionality that handles every varying parameter passed from the vertex shader to the pixel shader.

This way, any position on the surface has a texture coordinate attached to it. Just imagine how every 3D triangle of your geometry is mapped to a 2D triangle in UV space.

![UV mapping](_images\SingleVertexUv.png)

So all we need to do is to get access to the UV coordinates provided with the model and pass it through to the vertex shader. From there, put the UV coordinate unchanged into a varying variable thus passing it on to the pixel shader where we can use it already interpolated for the screen-pixel currently called for.

In code:
* In the vertex shader, add an `attribute vec2 fuUV` a varying `vec2 uv`, and a single line copying `fuUV` to `uv`. The entire vertex shader should look like this:
  ```glsl
  attribute vec3 fuVertex;
  attribute vec3 fuNormal;
  attribute vec2 fuUV;
  uniform mat4 FUSEE_MVP;
  uniform mat4 FUSEE_MV;
  uniform mat4 FUSEE_ITMV;
  varying vec3 viewpos;
  varying vec3 normal;
  varying vec2 uv;

  void main() {
    normal = normalize(mat3(FUSEE_ITMV) * fuNormal);
    viewpos = (FUSEE_MV * vec4(fuVertex, 1.0)).xyz;
    uv = fuUV;
    gl_Position = FUSEE_MVP * vec4(fuVertex, 1.0);
  }
  ```

* Finally, in the pixel shader, also declare `varying vec2 uv` and replace `vec(0,0)` as the parameter to `texture2D()` with `uv`.

When running the program you should now be able to explore WuggyLand's flora with its unique foliage.

![Finished](_images\WuggyLand_Finished.png)

## Effects = Shaders + Passes + Renderstates
To enable advanced visual effects it is often necessary to combine the output of several rendering passes - that is rendering the same geometry more than once with different shaders. Additionally, it is often necessary to switch other settings of the rendering pipeline between different passes. 

Such combinations of applying several render passes with different shaders and different settigns are very often called "Effects" (FX). Hence, why we are using FUSEE's support class called `ShaderEffect`, which allows us to define effects in a convenient way.

## Exercise
* Prepare your Renderer to handle geometry with more than one texture.
  * Implement a texture-lookup (using a `Dictionary<string, ITexture>` object).
* Prepare your Renderer to handle more than one `ShaderEffect` - e.g. based on object names.
  * In the material visitor lookup the `CurrentNode.Name` and read the respective effect from a `Dictionary<string, ShaderEffect>` object which can be filled in `Init()`.
* Implement a two-pass renderer drawing a black outline.
  * The first pass vertex shader transforms each vertex by MVP into clip-space and additionally moves it along the x- and y-coordinates of the normal (in clip-space).
  * The first pass pixel shader just assigns black to every pixel.
  * The first pass' `StateSet` should set `CullMode = Cull.Clockwise` and `ZEnable = false`.
  * The second pass should use the current vertex and pixel shader.
  * The second pass' `StateSet` should set `CullMode = Coll.Counterclockwise` and `ZEnabel = true`. Here is an example for such a set.
    #### Vertex Shader
    ```glsl
    attribute vec3 fuVertex;
    attribute vec3 fuNormal;

    varying vec3 normal;

    uniform mat4 FUSEE_MVP;
    uniform mat4 FUSEE_ITMV;

    uniform vec2 linewidth;

    void main()
    {
        normal = mat3(FUSEE_ITMV[0].xyz, FUSEE_ITMV[1].xyz, FUSEE_ITMV[2].xyz) * fuNormal;
        normal = normalize(normal);
        gl_Position = (FUSEE_MVP * vec4(fuVertex, 1.0) ) + vec4(linewidth * normal.xy, 0, 0); // + vec4(0, 0, 0.06, 0);
    }
    ```
    #### Pixel Shader
    ```glsl
    #ifdef GL_ES
    precision highp float;
    #endif

    uniform vec4 linecolor;

    void main()
    {
        gl_FragColor = linecolor;
    }
    ```
* Note that this outline-renderer only works with geometry with the following properties.
  * No overlapping inner parts of geometry is allowed.
  * All geometry must have continuous normals at its edges. No hard edges allowed. To simulate hard edges, prepare your geometry with bevelled edges with small radii.
* Create some sample geometry matching these requirements and render it during your game.
* Change the second pass to cel-rendering.
  * Completely remove the lighting calculation for diffuse and specular parts, instead:
    * Draw, render or download an image of a cartoon-like lit white sphere, like the one below. Note that less color fades and more sharp color borders create a more cartoonish look - so you might create better spheres than this.

    ![Sphere](_images\litsphere.jpg)

    * In the pixel shader, use this sphere image as a texture to retrieve the overall lighting intensity. Calculate the texture coordinates for this texture from the `varying vec3 normal` like this: `vec2 uv = normal.xy * 0.5 + vec2(0.5, 0.5);`
    * Combine the resulting intensity with the albedo from the material.