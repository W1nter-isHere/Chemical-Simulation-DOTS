using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    [BurstCompile]
    public struct CellComponent : IComponentData
    {
        public uint2 Position;
    }
}