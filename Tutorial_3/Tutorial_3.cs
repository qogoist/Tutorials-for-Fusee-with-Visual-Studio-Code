using System;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;
using Fusee.Engine.GUI;

namespace FuseeApp
{

    [FuseeApplication(Name = "Tutorial_2_Completed", Description = "Yet another FUSEE App.")]
    public class Tutorial_2_Completed : RenderCanvas
    {
        private Mesh _mesh;
        private ShaderEffect _shaderEffect;
        private string _vertexShader = AssetStorage.Get<string>("VertexShader.vert");
        private string _pixelShader = AssetStorage.Get<string>("PixelShader.frag");
        private float _alpha;

        // Init is called on startup. 
        public override void Init()
        {
            // Set the clear color for the backbuffer to white (100% intensity in all color channels R, G, B, A).
            RC.ClearColor = new float4(0, 1, 1, 1);

            // Create a new ShaderEffect based on the _vertexShader and _pixelShader and set it as the currently used ShaderEffect
            _shaderEffect = new ShaderEffect(
                new[]
                {
                    new EffectPassDeclaration{VS = _vertexShader, PS = _pixelShader, StateSet = new RenderStateSet{}}
                },
                new[]
                {
                    new EffectParameterDeclaration { Name = "DiffuseColor", Value = new float4(1, 1, 1, 1) },
                    new EffectParameterDeclaration { Name = "alpha", Value = _alpha }
                }
            );

            // Set _shader as the current ShaderEffect
            RC.SetShaderEffect(_shaderEffect);

            // Create a new Mesh 
            _mesh = new Mesh
            {
                Vertices = new[]
                {
                    // left, down, front vertex
                    new float3(-1, -1, -1), // 0  - belongs to left
                    new float3(-1, -1, -1), // 1  - belongs to down
                    new float3(-1, -1, -1), // 2  - belongs to front

                    // left, down, back vertex
                    new float3(-1, -1,  1),  // 3  - belongs to left
                    new float3(-1, -1,  1),  // 4  - belongs to down
                    new float3(-1, -1,  1),  // 5  - belongs to back

                    // left, up, front vertex
                    new float3(-1,  1, -1),  // 6  - belongs to left
                    new float3(-1,  1, -1),  // 7  - belongs to up
                    new float3(-1,  1, -1),  // 8  - belongs to front

                    // left, up, back vertex
                    new float3(-1,  1,  1),  // 9  - belongs to left
                    new float3(-1,  1,  1),  // 10 - belongs to up
                    new float3(-1,  1,  1),  // 11 - belongs to back

                    // right, down, front vertex
                    new float3( 1, -1, -1), // 12 - belongs to right
                    new float3( 1, -1, -1), // 13 - belongs to down
                    new float3( 1, -1, -1), // 14 - belongs to front

                    // right, down, back vertex
                    new float3( 1, -1,  1),  // 15 - belongs to right
                    new float3( 1, -1,  1),  // 16 - belongs to down
                    new float3( 1, -1,  1),  // 17 - belongs to back

                    // right, up, front vertex
                    new float3( 1,  1, -1),  // 18 - belongs to right
                    new float3( 1,  1, -1),  // 19 - belongs to up
                    new float3( 1,  1, -1),  // 20 - belongs to front

                    // right, up, back vertex
                    new float3( 1,  1,  1),  // 21 - belongs to right
                    new float3( 1,  1,  1),  // 22 - belongs to up
                    new float3( 1,  1,  1),  // 23 - belongs to back

                },
                Normals = new[]
                {
                    // left, down, front vertex
                    new float3(-1,  0,  0), // 0  - belongs to left
                    new float3( 0, -1,  0), // 1  - belongs to down
                    new float3( 0,  0, -1), // 2  - belongs to front

                    // left, down, back vertex
                    new float3(-1,  0,  0),  // 3  - belongs to left
                    new float3( 0, -1,  0),  // 4  - belongs to down
                    new float3( 0,  0,  1),  // 5  - belongs to back

                    // left, up, front vertex
                    new float3(-1,  0,  0),  // 6  - belongs to left
                    new float3( 0,  1,  0),  // 7  - belongs to up
                    new float3( 0,  0, -1),  // 8  - belongs to front

                    // left, up, back vertex
                    new float3(-1,  0,  0),  // 9  - belongs to left
                    new float3( 0,  1,  0),  // 10 - belongs to up
                    new float3( 0,  0,  1),  // 11 - belongs to back

                    // right, down, front vertex
                    new float3( 1,  0,  0), // 12 - belongs to right
                    new float3( 0, -1,  0), // 13 - belongs to down
                    new float3( 0,  0, -1), // 14 - belongs to front

                    // right, down, back vertex
                    new float3( 1,  0,  0),  // 15 - belongs to right
                    new float3( 0, -1,  0),  // 16 - belongs to down
                    new float3( 0,  0,  1),  // 17 - belongs to back

                    // right, up, front vertex
                    new float3( 1,  0,  0),  // 18 - belongs to right
                    new float3( 0,  1,  0),  // 19 - belongs to up
                    new float3( 0,  0, -1),  // 20 - belongs to front

                    // right, up, back vertex
                    new float3( 1,  0,  0),  // 21 - belongs to right
                    new float3( 0,  1,  0),  // 22 - belongs to up
                    new float3( 0,  0,  1),  // 23 - belongs to back
                },
                Triangles = new ushort[]
                {
                   0,  6,  3,     3,  6,  9, // left
                   2, 14, 20,     2, 20,  8, // front
                  12, 15, 18,    15, 21, 18, // right
                   5, 11, 17,    17, 11, 23, // back
                   7, 22, 10,     7, 19, 22, // top
                   1,  4, 16,     1, 16, 13, // bottom 
                },
            };
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            float2 speed = Mouse.Velocity + Touch.GetVelocity(TouchPoints.Touchpoint_0);
            if (Mouse.LeftButton || Touch.GetTouchActive(TouchPoints.Touchpoint_0))
                _alpha += speed.x * 0.0001f;

            _shaderEffect.SetEffectParam("alpha", _alpha);

            // Render the selected mesh, using the previously set ShaderEffect
            RC.Render(_mesh);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
        }

        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width / (float)Height;

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 0.01 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 200 (Anything further away from the camera than 200 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(M.PiOver4, aspectRatio, 0.01f, 200.0f);
            RC.Projection = projection;
        }
    }
}