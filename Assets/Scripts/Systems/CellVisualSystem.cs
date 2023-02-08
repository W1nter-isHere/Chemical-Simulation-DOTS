using Aspects;
using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Systems
{
    [BurstCompile]
    public partial struct CellVisualSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<CellVisualPrefabComponent>(out var cellVisualPrefab)) return;

            var grid = SystemAPI.GetSingleton<GridComponent>();
            var positions = SystemAPI.GetSingleton<CellPositionsComponent>().Grid;
            var allCells = positions.GetValueArray(Allocator.TempJob);
            var cellVisuals = new NativeList<TransformAspect>(Allocator.TempJob);
            
            foreach (var transformAspect in SystemAPI.Query<TransformAspect>().WithAll<CellVisualTag>())
            {
                cellVisuals.Add(transformAspect);
            }

            // if not enough cell visuals are present we instantiate them
            var missingVisualCount = allCells.Length - cellVisuals.Length;
            if (missingVisualCount > 0)
            {
                var newVisuals = new NativeArray<Entity>(missingVisualCount, Allocator.Temp);
                SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged)
                    .Instantiate(cellVisualPrefab.CellVisualPrefab, newVisuals);
                newVisuals.Dispose();
            }
            
            new UpdateVisualPositions
            {
                Cells = allCells,
                Visuals = cellVisuals,
                Grid = grid,
            }
                .Schedule(allCells.Length, 64)
                .Complete();

            allCells.Dispose();
            cellVisuals.Dispose();
        }
    }

    [BurstCompile]
    public struct UpdateVisualPositions : IJobParallelFor
    {
        [ReadOnly] public NativeArray<CellAspect> Cells;
        [ReadOnly] public GridComponent Grid;
        [ReadOnly] public NativeList<TransformAspect> Visuals;

        [BurstCompile]
        public void Execute(int index)
        {
            var transformAspect = Visuals[index];
            transformAspect.WorldPosition = Grid.GridToWorld(Cells[index].Cell.ValueRO.Position);
        }
    }
}