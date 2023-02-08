using System;
using Aspects;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Components
{
    public class CellPositionsAuthoring : MonoBehaviour
    {
        private class CellPositionsBaker : Baker<CellPositionsAuthoring>
        {
            public override void Bake(CellPositionsAuthoring authoring)
            {
                AddComponent(new CellPositionsComponent
                {
                    Grid = new NativeParallelHashMap<uint2, CellAspect>(256, Allocator.Persistent)
                });
            }
        }
    }

    public struct CellPositionsComponent : IComponentData
    {
        public NativeParallelHashMap<uint2, CellAspect> Grid;
    }
}