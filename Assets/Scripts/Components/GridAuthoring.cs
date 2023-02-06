using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Components
{
    /// <summary>
    /// Contains the grid information
    /// Actual cells stored on the grid are stored in CellComponent
    /// </summary>
    public class GridAuthoring : MonoBehaviour
    {
        public uint width;
        public uint height;
        public float cellWidth;
        public float cellHeight;
        public float offsetX;
        public float offsetY;

        public Vector3 GridToWorld(Vector2Int gridPosition)
        {
            return new Vector3(cellWidth * gridPosition.x + offsetX, cellHeight * gridPosition.y + offsetY);
        }
        
        public Vector2Int RestrictPosition(Vector2Int position)
        {
            return new Vector2Int((int) Math.Min(Math.Max(0, position.x), width - 1), (int) Math.Min(Math.Max(0, position.y), height - 1));
        }
        
        private class GridBaker : Baker<GridAuthoring>
        {
            public override void Bake(GridAuthoring authoring)
            {
                AddComponent(new GridComponent
                {
                    Width = authoring.width,
                    Height = authoring.height,
                    CellWidth = authoring.cellWidth,
                    CellHeight = authoring.cellHeight,
                    OffsetX = authoring.offsetX,
                    OffsetY = authoring.offsetY
                });
            }
        }
    }

    [BurstCompile]
    public struct GridComponent : IComponentData
    {
        public uint Width;
        public uint Height;
        public float CellWidth;
        public float CellHeight;
        public float OffsetX;
        public float OffsetY;

        [BurstCompile]
        public uint2 WorldToGrid(float3 worldPosition)
        {
            return new uint2((uint)((worldPosition.x - OffsetX) / CellWidth), (uint)((worldPosition.y - OffsetY) / CellHeight));
        }
        
        [BurstCompile]
        public float3 GridToWorld(uint2 gridPosition)
        {
            return new float3(CellWidth * gridPosition.x + OffsetX, CellHeight * gridPosition.y + OffsetY, 0);
        }

        [BurstCompile]
        public uint2 RestrictPosition(uint2 position)
        {
            return new uint2(math.min(math.max(0, position.x), Width - 1), math.min(math.max(0, position.y), Height - 1));
        }

        [BurstCompile]
        public bool ValidPosition(uint2 position)
        {
            return position.x < Width && position.y < Height;
        }
    }
}