using Unity.Entities;
using UnityEngine;

namespace Components
{
    public class CellVisualAuthoring : MonoBehaviour
    {
        private class CellVisualBaker : Baker<CellVisualAuthoring>
        {
            public override void Bake(CellVisualAuthoring authoring)
            {
                AddComponent<CellVisualTag>();
            }
        }
    }
    
    public struct CellVisualTag : IComponentData
    {
    }
}