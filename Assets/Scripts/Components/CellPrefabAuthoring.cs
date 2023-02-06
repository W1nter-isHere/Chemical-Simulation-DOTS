using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Components
{
    public class CellPrefabAuthoring : MonoBehaviour
    {
        public GameObject cellPrefab;
        
        private class CellPrefabBaker : Baker<CellPrefabAuthoring>
        {
            public override void Bake(CellPrefabAuthoring authoring)
            {
                AddComponent(new CellPrefabComponent
                {
                    CellPrefab = GetEntity(authoring.cellPrefab)
                });
            }
        }
    }

    [BurstCompile]
    public struct CellPrefabComponent : IComponentData
    {
        public Entity CellPrefab;
    }
}