using Unity.Entities;
using UnityEngine;

namespace Components
{
    public class CellMaterialAuthoring : MonoBehaviour
    {
        
    }

    public class CellMaterialBaker : Baker<CellMaterialAuthoring>
    {
        public override void Bake(CellMaterialAuthoring authoring)
        {
            AddComponent(new CellMaterialComponent());
        }
    }

    public struct CellMaterialComponent : IComponentData
    {
        
    }
}