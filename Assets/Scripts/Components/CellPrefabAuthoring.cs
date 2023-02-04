using Unity.Entities;
using UnityEngine;

namespace Components
{
    public class CellPrefabAuthoring : MonoBehaviour
    {
        public GameObject cellPrefab;
    }

    public class CellPrefabBaker : Baker<CellPrefabAuthoring>
    {
        public override void Bake(CellPrefabAuthoring authoring)
        {
            AddComponent(new CellPrefabComponent
            {
                CellPrefab = GetEntity(authoring.cellPrefab)
            });
        }
    }

    public struct CellPrefabComponent : IComponentData
    {
        public Entity CellPrefab;
    }
}