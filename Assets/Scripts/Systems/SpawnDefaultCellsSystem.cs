using Aspects;
using Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Systems
{
    [UpdateAfter(typeof(SetupGridSystem))]
    public partial class SpawnDefaultCellsSystem : SystemBase
    {
        private bool _initialized;

        protected override void OnUpdate()
        {
            // if (_initialized) return;
            // if (!SystemAPI.TryGetSingleton<GridComponent>(out var grid)) return;
            // var cellPrefab = SystemAPI.GetSingleton<CellPrefabComponent>();
            //
            // for (uint i = 0; i < grid.Width; i++)
            // {
            //     for (uint j = 0; j < grid.Height; j++)
            //     {
            //         if (j % 2 == 0) continue;
            //         var cellAspect = SystemAPI.GetAspectRW<CellAspect>(EntityManager.Instantiate(cellPrefab.CellPrefab));
            //         cellAspect.Move(grid, new uint2(i, j));
            //         cellAspect.Material.ValueRW.CellType = CellType.FallingSolid;
            //     }
            // }
            //
            // _initialized = true;
            //
            // Debug.Log(EntityManager.CreateEntityQuery(typeof(CellComponent)).CalculateEntityCount().ToString());
        }
    }
}