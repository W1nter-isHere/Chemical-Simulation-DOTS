using Aspects;
using Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Systems
{
    public partial class SpawnCellSystem : SystemBase
    {
        private Camera _camera;

        protected override void OnCreate()
        {
            _camera = Camera.main;
        }

        protected override void OnUpdate()
        {
            if (!Input.GetMouseButton(0)) return;
            
            var position = _camera.ScreenToWorldPoint(Input.mousePosition);
            var mousePosition = new float3(position.x, position.y, 0);
            
            var grid = SystemAPI.GetSingleton<GridComponent>();
            var cellPrefab = SystemAPI.GetSingleton<CellPrefabComponent>();
            
            var gridPosition = grid.WorldToGrid(mousePosition);
            if (!grid.ValidPosition(gridPosition)) return;

            var cell = SystemAPI.GetAspectRW<CellAspect>(EntityManager.Instantiate(cellPrefab.CellPrefab));
            cell.Move(grid, gridPosition);
        }
    }
}