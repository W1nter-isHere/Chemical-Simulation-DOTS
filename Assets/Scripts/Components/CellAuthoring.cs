using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Components
{
    public class CellAuthoring : MonoBehaviour
    {
        public Vector2Int position;

        private void OnValidate()
        {
            var grid = FindObjectOfType<GridAuthoring>();
            if (grid == null) return;
            position = grid.RestrictPosition(position);
            transform.position = grid.GridToWorld(position);
        }
    }

    public class CellBaker : Baker<CellAuthoring>
    {
        public override void Bake(CellAuthoring authoring)
        {
            AddComponent(new CellComponent
            {
                Position = new uint2((uint)authoring.position.x, (uint)authoring.position.y)
            });
        }
    }

    public struct CellComponent : IComponentData
    {
        public uint2 Position;
    }

    public enum CellType
    {
        StationarySolid,
        FallingSolid,
        Liquid,
        Gas
    }
}