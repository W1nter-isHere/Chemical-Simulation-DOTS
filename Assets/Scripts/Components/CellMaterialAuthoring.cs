using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Components
{
    public class CellMaterialAuthoring : MonoBehaviour
    {
        public CellType cellType;
        
        private class CellMaterialBaker : Baker<CellMaterialAuthoring>
        {
            public override void Bake(CellMaterialAuthoring authoring)
            {
                AddComponent(new CellMaterialComponent
                {
                    CellType = authoring.cellType
                });
            }
        }
    }

    [BurstCompile]
    public struct CellMaterialComponent : IComponentData
    {
        public CellType CellType;
    }
}