using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Components
{
    public class CellAuthoring : MonoBehaviour
    {
        public Vector2Int position;
        public CellType cellType;

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
                Position = new uint2((uint)authoring.position.x, (uint)authoring.position.y),
                CellType = authoring.cellType
            });
        }
    }

    public struct CellComponent : IComponentData
    {
        public uint2 Position;
        public CellType CellType;
    }

    public enum CellType
    {
        Solid,
        Liquid,
        Gas
    }
}