using System;
using System.Collections.Generic;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;

class Renderer : SceneVisitor
{
    public RenderContext RC;
    public float4x4 View;
    private CollapsingStateStack<float4x4> _model = new CollapsingStateStack<float4x4>();

    public Renderer(RenderContext rc)
    {
        RC = rc;

        //Initialize shaders
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
                new EffectParameterDeclaration { Name = "shininess", Value = 0 }
            }
        );
        RC.SetShaderEffect(shaderEffect);

    }

    [VisitMethod]
    void Onmesh(Mesh mesh)
    {
        RC.Render(mesh);
    }

    [VisitMethod]
    void OnShaderEffect(ShaderEffectComponent shader)
    {
        RC.SetFXParam("albedo", shader.Effect.GetEffectParam("DiffuseColor"));
        RC.SetFXParam("shininess", shader.Effect.GetEffectParam("SpecularShininess"));
    }

    [VisitMethod]
    void OnTransform(TransformComponent xform)
    {
        _model.Tos *= xform.Matrix();
        RC.ModelView = View * _model.Tos;
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

    protected override void InitState()
    {
        _model.Clear();
        _model.Tos = float4x4.Identity;
    }
}