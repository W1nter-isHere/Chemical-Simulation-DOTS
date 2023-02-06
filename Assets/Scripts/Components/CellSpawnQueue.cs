using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    [BurstCompile]
    public struct CellSpawnQueue : IBufferElementData
    {
        public float3 Position;
    }
}