using System;
using System.Collections.Generic;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;
using Fusee.Engine.GUI;
using System.Linq;
using Fusee.Xene;

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
        private float _beta;
        private float4x4 _xform;

        private float _yawCube1;
        private float _pitchCube1;
        private float _yawCube2;
        private float _pitchCube2;

        // Init is called on startup. 
        public override void Init()
        {
            // Set the clear color for the backbuffer to white (100% intensity in all color channels R, G, B, A).
            RC.ClearColor = new float4(1, 1, 1, 1);

            _xform = float4x4.Identity;

            // Create a new ShaderEffect based on the _vertexShader and _pixelShader and set it as the currently used ShaderEffect
            _shaderEffect = new ShaderEffect(
                new[]
                {
                    new EffectPassDeclaration{VS = _vertexShader, PS = _pixelShader, StateSet = new RenderStateSet{}}
                },
                new[]
                {
                    new EffectParameterDeclaration { Name = "DiffuseColor", Value = new float4(1, 1, 1, 1) },
                    new EffectParameterDeclaration { Name = "xform", Value = _xform }
                }
            );

            // Set _shader as the current ShaderEffect
            RC.SetShaderEffect(_shaderEffect);

            //Load the scene file "Cube.fus"
            SceneContainer scene = AssetStorage.Get<SceneContainer>("Cube.fus");

            //Extract the First object of type Mesh found in scene's list of Children.
            _mesh = scene.Children.FindComponents<Mesh>(c => true).First();
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            float2 speed = Mouse.Velocity + Touch.GetVelocity(TouchPoints.Touchpoint_0);
            if (Mouse.LeftButton || Touch.GetTouchActive(TouchPoints.Touchpoint_0))
            {
                _alpha -= speed.x * 0.0001f;
                _beta -= speed.y * 0.0001f;
            }

            _yawCube1 -= Keyboard.ADAxis * 0.1f;
            _pitchCube1 += Keyboard.WSAxis * 0.1f;
            _yawCube2 -= Keyboard.LeftRightAxis * 0.1f;
            _pitchCube2 += Keyboard.UpDownAxis * 0.1f;

            //Setip matrices
            var aspectRatio = Width / (float)Height;
            var projection = float4x4.CreatePerspectiveFieldOfView(M.Pi * 0.25f, aspectRatio, 0.01f, 20);
            var view = float4x4.CreateTranslation(0, 0, 3) * float4x4.CreateRotationY(_alpha) * float4x4.CreateRotationX(_beta);

            //First cube
            var cube1Model = ModelXForm(new float3(-0.6f, 0, 0), new float3(_pitchCube1, _yawCube1, 0), new float3(0, 0, 0));
            _xform = projection * view * cube1Model * float4x4.CreateScale(0.5f, 0.1f, 0.1f);
            _shaderEffect.SetEffectParam("xform", _xform);
            RC.Render(_mesh);

            //Second cube
            var cube2Model = ModelXForm(new float3(1.0f, 0, 0), new float3(_pitchCube2, _yawCube2, 0), new float3(-0.5f, 0, 0));
            _xform = projection * view * cube1Model * cube2Model * float4x4.CreateScale(0.5f, 0.1f, 0.1f);
            _shaderEffect.SetEffectParam("xform", _xform);
            RC.Render(_mesh);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
        }

        static float4x4 ModelXForm(float3 pos, float3 rot, float3 pivot)
        {
            return float4x4.CreateTranslation(pos + pivot) * float4x4.CreateRotationY(rot.y) * float4x4.CreateRotationX(rot.x)
                    * float4x4.CreateRotationZ(rot.z) * float4x4.CreateTranslation(-pivot);
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