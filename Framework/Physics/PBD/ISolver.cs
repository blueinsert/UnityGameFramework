using bluebean.UGFramework.DataStruct;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace bluebean.UGFramework.Physics
{
    public interface ISolver
    {
        NativeArray<float4> ParticlePositions { get; }
         NativeArray<float> InvMasses { get; }
        NativeArray<float4> PositionDeltas { get; }

        NativeArray<float4> Gradients { get; }

        NativeArray<int> PositionConstraintCounts { get; }
    }
}