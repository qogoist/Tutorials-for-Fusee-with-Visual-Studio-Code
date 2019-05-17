﻿using System;
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

    [FuseeApplication(Name = "Tutorial_1_Completed", Description = "Yet another FUSEE App.")]
    public class Tutorial_1_Completed : RenderCanvas
    {
        private ShaderEffect _shader;
        private Mesh _mesh;
        private const string _vertexShader = @"
            attribute vec3 fuVertex;
        
            void main()
            {
                gl_Position = vec4(fuVertex, 1.0);
            }";
        private const string _pixelShader = @"
            #ifdef GL_ES
                precision highp float;
            #endif
            
            void main()
            {
                gl_FragColor = vec4(1, 0, 1, 1);
            }";

        // Init is called on startup. 
        public override void Init()
        {
            // Set the clear color for the backbuffer to white (100% intensity in all color channels R, G, B, A).
            RC.ClearColor = new float4(0, 1, 1, 1);

            // Create a new ShaderEffect based on the _vertexShader and _pixelShader and set it as the currently used ShaderEffect
            _shader = SimpleShaders.MakeShader(_vertexShader, _pixelShader);
            RC.SetShaderEffect(_shader);

            // Create a new Mesh 
            _mesh = new Mesh
            {
                Vertices = new[]
                {
                    new float3(-0.5f, -0.5f, 0),
                    new float3(0.5f, -0.5f, 0),
                    new float3(0, 0.5f, 0),
                },
                Triangles = new ushort[] { 0, 1, 2 },
            };
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

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