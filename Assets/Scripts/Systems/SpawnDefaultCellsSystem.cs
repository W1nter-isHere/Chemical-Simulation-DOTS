using Aspects;
using Components;
using Unity.Entities;
using Unity.Mathematics;

namespace Systems
{
    public partial class SpawnDefaultCellsSystem : SystemBase
    {
        protected override void OnStartRunning()
        {
            var grid = SystemAPI.GetSingleton<GridComponent>();
            var cellPrefab = SystemAPI.GetSingleton<CellPrefabComponent>();

            for (uint i = 0; i < grid.Width; i++)
            {
                for (uint j = 0; j < grid.Height; j++)
                {
                    if (j % 2 == 0) continue;
                    var cellAspect = SystemAPI.GetAspectRW<CellAspect>(EntityManager.Instantiate(cellPrefab.CellPrefab));
                    cellAspect.Move(grid, new uint2(i, j));
                }
            }
        }

        protected override void OnUpdate()
        {
        }
    }
}