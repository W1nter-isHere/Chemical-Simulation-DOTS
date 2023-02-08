using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Components
{
    public class CellVisualPrefabAuthoring : MonoBehaviour
    {
        public GameObject cellVisualPrefab;
        
        private class CellPrefabBaker : Baker<CellVisualPrefabAuthoring>
        {
            public override void Bake(CellVisualPrefabAuthoring prefabAuthoring)
            {
                AddComponent(new CellVisualPrefabComponent
                {
                    CellVisualPrefab = GetEntity(prefabAuthoring.cellVisualPrefab)
                });
            }
        }
    }
    
    [BurstCompile]
    public struct CellVisualPrefabComponent : IComponentData
    {
        public Entity CellVisualPrefab;
    }
}