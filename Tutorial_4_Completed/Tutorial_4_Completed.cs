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

    [FuseeApplication(Name = "Tutorial_4_Completed", Description = "Yet another FUSEE App.")]
    public class Tutorial_4_Completed : RenderCanvas
    {
        private Mesh _mesh;
        private ShaderEffect _shaderEffect;
        private SceneOb _root;
        private string _vertexShader = AssetStorage.Get<string>("VertexShader.vert");
        private string _pixelShader = AssetStorage.Get<string>("PixelShader.frag");
        private float _alpha;
        private float _beta;
        private float _yawCube1;
        private float _pitchCube1;
        private float _yawCube2;
        private float _pitchCube2;

        // Init is called on startup. 
        public override void Init()
        {
            // Initialize shader(s)
            _shaderEffect = new ShaderEffect(
                new[]
                {
                    new EffectPassDeclaration{VS = _vertexShader, PS = _pixelShader, StateSet = new RenderStateSet{}}
                },
                new[]
                {
                    new EffectParameterDeclaration { Name = "DiffuseColor", Value = new float4(1, 1, 1, 1) },
                    new EffectParameterDeclaration { Name = "albedo", Value = float3.One }
                }
            );

            RC.SetShaderEffect(_shaderEffect);

            // Load some meshes
            Mesh cone = LoadMesh("Cone.fus");
            Mesh cube = LoadMesh("Cube.fus");
            Mesh cylinder = LoadMesh("Cylinder.fus");
            Mesh pyramid = LoadMesh("Pyramid.fus");
            Mesh sphere = LoadMesh("Sphere.fus");

            // Setup a list of objects
            _root = new SceneOb {
                Children = new List<SceneOb>(new[]
                {
                    //Body
                    new SceneOb {Mesh = cube, Pos = new float3(0, 2.75f, 0), ModelScale = new float3(0.5f, 1, 0.25f)},
                    //Legs
                    new SceneOb {Mesh = cylinder, Pos = new float3(-0.25f, 1, 0), ModelScale = new float3(0.15f, 1, 0.15f)},
                    new SceneOb {Mesh = cylinder, Pos = new float3(0.25f, 1, 0), ModelScale = new float3(0.15f, 1, 0.15f)},
                    //Shoulders
                    new SceneOb {Mesh = sphere, Pos = new float3(-0.75f, 3.5f, 0), ModelScale = new float3(0.25f, 0.25f, 0.25f)},
                    new SceneOb {Mesh = sphere, Pos = new float3(0.75f, 3.5f, 0), ModelScale = new float3(0.25f, 0.25f, 0.25f)},
                    //Arms
                    new SceneOb {Mesh = cylinder, Pos = new float3(-0.75f, 2.5f, 0), ModelScale = new float3(0.15f, 1, 0.15f)},
                    new SceneOb {Mesh = cylinder, Pos = new float3(0.75f, 2.5f, 0), ModelScale = new float3(0.15f, 1, 0.15f)},
                    //Head
                    new SceneOb {Mesh = sphere, Pos = new float3(0, 4.2f, 0), ModelScale = new float3(0.35f, 0.5f, 0.35f)}
                })
            };

            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(1, 1, 1, 1);
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

            //Setup matrices
            var aspectRatio = Width / (float)Height;
            RC.Projection = float4x4.CreatePerspectiveFieldOfView(M.Pi * 0.25f, aspectRatio, 0.01f, 20);
            var view = float4x4.CreateTranslation(0, 0, 8) * float4x4.CreateRotationY(_alpha) * float4x4.CreateRotationX(_beta) * float4x4.CreateTranslation(0, -2, 0);

            RenderSceneOb(_root, view);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
        }

        static float4x4 ModelXForm(float3 pos, float3 rot, float3 pivot)
        {
            return float4x4.CreateTranslation(pos + pivot) * float4x4.CreateRotationY(rot.y) * float4x4.CreateRotationX(rot.x)
                    * float4x4.CreateRotationZ(rot.z) * float4x4.CreateTranslation(-pivot);
        }

        public static Mesh LoadMesh(string assetName)
        {
            SceneContainer scene = AssetStorage.Get<SceneContainer>(assetName);
            return scene.Children.FindComponents<Mesh>(c => true).First();
        }

        void RenderSceneOb(SceneOb so, float4x4 modelView)
        {
            modelView = modelView * ModelXForm(so.Pos, so.Rot, so.Pivot) * float4x4.CreateScale(so.Scale);
            if (so.Mesh != null)
            {
                RC.ModelView = modelView * float4x4.CreateScale(so.ModelScale);
                _shaderEffect.SetEffectParam("albedo", so.Albedo);
                RC.Render(so.Mesh);
            }

            if (so.Children != null)
            {
                foreach (var child in so.Children)
                {
                    RenderSceneOb(child, modelView);
                }
            }
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