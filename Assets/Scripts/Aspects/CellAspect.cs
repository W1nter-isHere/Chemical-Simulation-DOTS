using Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Aspects
{
    public readonly partial struct CellAspect : IAspect
    {
        public readonly Entity Entity;
        
        public readonly RefRW<CellMaterialComponent> Material;
        public readonly RefRW<CellComponent> Cell;

        public void Move([ReadOnly] GridComponent gridComponent, [ReadOnly] uint2 newPosition)
        {
            Cell.ValueRW.Position = gridComponent.RestrictPosition(newPosition);
        }
    }
}