using Unity.Entities;
using UnityEngine;

namespace Components
{
    public class CellMaterialAuthoring : MonoBehaviour
    {
        public CellType cellType;
    }

    public class CellMaterialBaker : Baker<CellMaterialAuthoring>
    {
        public override void Bake(CellMaterialAuthoring authoring)
        {
            AddComponent(new CellMaterialComponent
            {
                CellType = authoring.cellType
            });
        }
    }

    public struct CellMaterialComponent : IComponentData
    {
        public CellType CellType;
    }
}