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
        private float _alpha;

        // Init is called on startup. 
        public override void Init()
        {
            // Set the clear color for the backbuffer to white (100% intensity in all color channels R, G, B, A).
            RC.ClearColor = new float4(0, 1, 1, 1);

            var vertexShader = AssetStorage.Get<string>("VertexShader.vert");
            var pixelShader = AssetStorage.Get<string>("PixelShader.frag");

            _alpha = 0;

            // Create a new ShaderEffect based on the _vertexShader and _pixelShader and set it as the currently used ShaderEffect
            var shaderEffect = new ShaderEffect(
                new[]
                {
                    new EffectPassDeclaration{VS = vertexShader, PS = pixelShader, StateSet = new RenderStateSet{}}
                },
                new[]
                {
                    new EffectParameterDeclaration { Name = "DiffuseColor", Value = new float4(1, 1, 1, 1) },
                    new EffectParameterDeclaration { Name = "alpha", Value = _alpha }
                }
            );

            // Set _shader as the current ShaderEffect
            RC.SetShaderEffect(shaderEffect);

            // Create a new Mesh 
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
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            float2 speed = Mouse.Velocity;

            _alpha += speed.x * 0.0001f;
            RC.SetFXParam("alpha", _alpha);

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