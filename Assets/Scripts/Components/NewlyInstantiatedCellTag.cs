using Unity.Burst;
using Unity.Entities;

namespace Components
{
    [BurstCompile]
    public struct NewlyInstantiatedCellTag : IComponentData
    {
    }
}