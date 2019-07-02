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
using static System.Math;
using Fusee.Engine.GUI;
using System.Linq;
using Fusee.Xene;

namespace FuseeApp
{

    class Renderer : SceneVisitor
    {
        public RenderContext RC;
        public float4x4 View;
        private CollapsingStateStack<float4x4> _model = new CollapsingStateStack<float4x4>();

        public Renderer(RenderContext rc)
        {
            RC = rc;

            var vertexShader = AssetStorage.Get<string>("VertexShader.vert");
            var pixelShader = AssetStorage.Get<string>("PixelShader.frag");
            var shaderEffect = new ShaderEffect(
                new[]
                {
                    new EffectPassDeclaration{VS = vertexShader, PS = pixelShader, StateSet = new RenderStateSet{}}
                },
                new[]
                {
                    new EffectParameterDeclaration { Name = "albedo", Value = float3.One },
                    new EffectParameterDeclaration { Name = "shininess", Value = 0 },
                    new EffectParameterDeclaration { Name = "specfactor", Value = 0 },
                    new EffectParameterDeclaration { Name = "speccolor", Value = float3.One },
                }
            );
            RC.SetShaderEffect(shaderEffect);
        }

        protected override void InitState()
        {
            _model.Clear();
            _model.Tos = float4x4.Identity;
        }
        protected override void PushState()
        {
            _model.Push();
        }
        protected override void PopState()
        {
            _model.Pop();
            RC.ModelView = View * _model.Tos;
        }

        [VisitMethod]
        void OnMesh(Mesh mesh)
        {
            RC.Render(mesh);
        }
        [VisitMethod]
        void OnShaderEffect(ShaderEffectComponent shader)
        {
            RC.SetFXParam("shininess", shader.Effect.GetEffectParam("SpecularShininess"));
            RC.SetFXParam("specfactor", shader.Effect.GetEffectParam("SpecularIntensity"));
            RC.SetFXParam("speccolor", shader.Effect.GetEffectParam("SpecularColor"));
            RC.SetFXParam("albedo", shader.Effect.GetEffectParam("DiffuseColor"));
        }
        [VisitMethod]
        void OnTransform(TransformComponent xform)
        {
            _model.Tos *= xform.Matrix();
            RC.ModelView = View * _model.Tos;
        }
    }

    [FuseeApplication(Name = "Tutorial_6", Description = "Yet another FUSEE App.")]
    public class Tutorial_6 : RenderCanvas
    {
        // angle variables
        private static float _angleHorz = M.PiOver6 * 2.0f, _angleVert = -M.PiOver6 * 0.5f, _angleVelHorz, _angleVelVert, _angleRoll, _angleRollInit, _zoomVel, _zoom;
        private static float2 _offset;
        private static float2 _offsetInit;

        private const float RotationSpeed = 7;
        private const float Damping = 0.8f;

        private SceneContainer _scene;
        private float4x4 _sceneCenter;
        private float4x4 _sceneScale;
        private float4x4 _projection;
        private bool _twoTouchRepeated;

        private bool _keys;

        private TransformComponent _wuggyTransform;
        private TransformComponent _wgyWheelFrontRight;
        private TransformComponent _wgyWheelFrontLeft;
        private TransformComponent _wgyWheelBackRight;
        private TransformComponent _wgyWheelBackLeft;
        private TransformComponent _wgyAxleBack;
        private TransformComponent _wgyAxleFront;
        private TransformComponent _wgyNeckHi;
        private List<SceneNodeContainer> _trees;

        private Renderer _renderer;


        //Init is called on startup.
        public override void Init()
        {
            //Load the scene
            _scene = AssetStorage.Get<SceneContainer>("WuggyLand.fus");
            _sceneScale = float4x4.CreateScale(1.0f);


            //Initiate the Renderer
            _renderer = new Renderer(RC);

            //Find some transform nodes we want to manipulate in the scene
            _wuggyTransform = _scene.Children.FindNodes(c => c.Name == "Wuggy").First()?.GetTransform();
            _wgyWheelFrontRight = _scene.Children.FindNodes(c => c.Name == "Wheel_Front_Right").First()?.GetTransform();
            _wgyWheelFrontLeft = _scene.Children.FindNodes(c => c.Name == "Wheel_Front_Left").First()?.GetTransform();
            _wgyWheelBackRight = _scene.Children.FindNodes(c => c.Name == "Wheel_Back_Right").First()?.GetTransform();
            _wgyWheelBackLeft = _scene.Children.FindNodes(c => c.Name == "Wheel_Back_Left").First()?.GetTransform();
            _wgyAxleBack = _scene.Children.FindNodes(c => c.Name == "Axle_Back").First()?.GetTransform();
            _wgyAxleFront = _scene.Children.FindNodes(c => c.Name == "Axle_Front").First()?.GetTransform();
            _wgyNeckHi = _scene.Children.FindNodes(c => c.Name == "Neck_High").First()?.GetTransform();

            //Find trees and storethem in a list
            _trees = new List<SceneNodeContainer>();
            _trees.AddRange(_scene.Children.FindNodes(c => c.Name.Contains("Tree")));

            //Set the clear color for the backbuffer
            RC.ClearColor = new float4(1, 1, 1, 1);
        }

        //RenderAFrame is calledo nce a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            // Mouse and keyboard movement
            if (Keyboard.LeftRightAxis != 0 || Keyboard.UpDownAxis != 0)
            {
                _keys = true;
            }

            var curDamp = (float)System.Math.Exp(-Damping * DeltaTime);

            // Zoom & Roll
            if (Touch.TwoPoint)
            {
                if (!_twoTouchRepeated)
                {
                    _twoTouchRepeated = true;
                    _angleRollInit = Touch.TwoPointAngle - _angleRoll;
                    _offsetInit = Touch.TwoPointMidPoint - _offset;
                }
                _zoomVel = Touch.TwoPointDistanceVel * -0.01f;
                _angleRoll = Touch.TwoPointAngle - _angleRollInit;
                _offset = Touch.TwoPointMidPoint - _offsetInit;
            }
            else
            {
                _twoTouchRepeated = false;
                _zoomVel = Mouse.WheelVel * -0.05f;
                _angleRoll *= curDamp * 0.8f;
                _offset *= curDamp * 0.8f;
            }

            // UpDown / LeftRight rotation
            if (Mouse.LeftButton)
            {
                _keys = false;
                _angleVelHorz = -RotationSpeed * Mouse.XVel * 0.000002f;
                _angleVelVert = -RotationSpeed * Mouse.YVel * 0.000002f;
            }
            else if (Touch.GetTouchActive(TouchPoints.Touchpoint_0) && !Touch.TwoPoint)
            {
                _keys = false;
                float2 touchVel;
                touchVel = Touch.GetVelocity(TouchPoints.Touchpoint_0);
                _angleVelHorz = -RotationSpeed * touchVel.x * 0.000002f;
                _angleVelVert = -RotationSpeed * touchVel.y * 0.000002f;
            }
            else
            {
                if (_keys)
                {
                    _angleVelHorz = -RotationSpeed * Keyboard.LeftRightAxis * 0.002f;
                    _angleVelVert = -RotationSpeed * Keyboard.UpDownAxis * 0.002f;
                }
                else
                {
                    _angleVelHorz *= curDamp;
                    _angleVelVert *= curDamp;
                }
            }

            float wuggyYawSpeed = Keyboard.WSAxis * Keyboard.ADAxis * 0.03f;
            float wuggySpeed = Keyboard.WSAxis * -0.5f;

            //Wuggy XForm
            float wuggyYaw = _wuggyTransform.Rotation.y;
            wuggyYaw += wuggyYawSpeed;
            wuggyYaw = NormRot(wuggyYaw);

            float3 wuggyPos = _wuggyTransform.Translation;
            wuggyPos += new float3((float)Sin(wuggyYaw), 0, (float)Cos(wuggyYaw)) * wuggySpeed;

            _wuggyTransform.Rotation = new float3(0, wuggyYaw, 0);
            _wuggyTransform.Translation = wuggyPos;

            //Wuggy Wheels
            _wgyAxleFront.Rotation += new float3(wuggySpeed * 0.09f, 0, 0);
            _wgyAxleBack.Rotation = new float3(_wgyAxleBack.Rotation.x + wuggySpeed * 0.18f, -Keyboard.ADAxis * 0.3f, 0);


            SceneNodeContainer target = GetClosest();
            float camYaw = 0;
            if (target != null)
            {
                float3 delta = target.GetTransform().Translation - _wuggyTransform.Translation;
                camYaw = (float)Atan2(-delta.x, -delta.z) - _wuggyTransform.Rotation.y;
            }

            camYaw = NormRot(camYaw);
            float deltaAngle = camYaw - _wgyNeckHi.Rotation.y;
            if (deltaAngle > M.Pi)
                deltaAngle = deltaAngle - M.TwoPi;
            if (deltaAngle < -M.Pi)
                deltaAngle = deltaAngle + M.TwoPi;
            var newYaw = _wgyNeckHi.Rotation.y + (float)M.Clamp(deltaAngle, -0.06, 0.06);
            newYaw = NormRot(newYaw);
            _wgyNeckHi.Rotation = new float3(0, newYaw, 0);

            _zoom += _zoomVel;
            // Limit zoom
            if (_zoom < 20)
                _zoom = 20;
            if (_zoom > 500)
                _zoom = 500;

            _angleHorz += _angleVelHorz;
            // Wrap-around to keep _angleHorz between -PI and + PI
            _angleHorz = M.MinAngle(_angleHorz);

            _angleVert += _angleVelVert;
            // Limit pitch to the range between [-PI/2, + PI/2]
            _angleVert = M.Clamp(_angleVert, -M.PiOver2, M.PiOver2);

            // Wrap-around to keep _angleRoll between -PI and + PI
            _angleRoll = M.MinAngle(_angleRoll);


            // Create the camera matrix and set it as the current ModelView transformation
            var mtxRot = float4x4.CreateRotationZ(_angleRoll) * float4x4.CreateRotationX(_angleVert) * float4x4.CreateRotationY(_angleHorz);
            var mtxCam = float4x4.LookAt(0, 20, -_zoom, 0, 0, 0, 0, 1, 0);
            _renderer.View = mtxCam * mtxRot * _sceneScale;
            var mtxOffset = float4x4.CreateTranslation(2 * _offset.x / Width, -2 * _offset.y / Height, 0);
            RC.Projection = mtxOffset * _projection;


            _renderer.Traverse(_scene.Children);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rerndered farame) on the front buffer.
            Present();

        }

        private SceneNodeContainer GetClosest()
        {
            float minDist = float.MaxValue;
            SceneNodeContainer ret = null;
            foreach (var target in _trees)
            {
                var xf = target.GetTransform();
                float dist = (_wuggyTransform.Translation - xf.Translation).Length;
                if (dist < minDist)
                {
                    ret = target;
                    minDist = dist;
                }
            }
            return ret;
        }

        public static float NormRot(float rot)
        {
            while (rot > M.Pi)
                rot -= M.TwoPi;
            while (rot < -M.Pi)
                rot += M.TwoPi;
            return rot;
        }

        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width / (float)Height;

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 0.01 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 200 (Anything further away from the camera than 200 world units gets clipped, polygons will be cut)
            _projection = float4x4.CreatePerspectiveFieldOfView(M.PiOver4, aspectRatio, 0.01f, 200.0f);
        }
    }
}