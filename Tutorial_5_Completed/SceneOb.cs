using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using System.Collections.Generic;

namespace FuseeApp
{
    public class SceneOb
    {
        public Mesh Mesh;
        public float3 Albedo = new float3(0.8f, 0.8f, 0.8f);
        public float3 Pos = float3.Zero;
        public float3 Rot = float3.Zero;
        public float3 Pivot = float3.Zero;
        public float3 Scale = float3.One;
        public float3 ModelScale = float3.One;
        public List<SceneOb> Children;
    }
}