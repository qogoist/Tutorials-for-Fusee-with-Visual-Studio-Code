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
using System.Collections.Generic;

namespace FuseeApp
{
    public static class SimpleShaders
    {
        public static ShaderEffect MakeShader(string vs, string ps)
        {
            return new ShaderEffect(
                new[] 
                { 
                    new EffectPassDeclaration { VS = vs, PS = ps, StateSet = new RenderStateSet { } } 
                }, 
                new[] 
                { 
                    new EffectParameterDeclaration { Name = "DiffuseColor", Value = float4.One } 
                });
        }
    }
}