using Aspects;
using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Utils;

namespace Systems
{
    [BurstCompile]
    public partial struct UpdateCellsSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var grid = SystemAPI.GetSingleton<GridComponent>();
            var cellCounts = (int) (grid.Width * grid.Height);
            
            var cells = new NativeArray<CellAspect>(cellCounts, Allocator.TempJob);
            var cellOccupationState = new NativeArray<bool>(cellCounts, Allocator.TempJob);
            
            foreach (var cellAspect in SystemAPI.Query<CellAspect>())
            {
                var pos = cellAspect.Cell.ValueRO.Position;
                var i = (int) MathUtilities.FlattenToIndex(pos.x, pos.y, grid.Width);
                cells[i] = cellAspect;
                cellOccupationState[i] = true;
            }

            var job = new UpdateJob
            {
                CellArray = cells,
                CellOccupied = cellOccupationState,
                Grid = grid
            }.ScheduleParallel(state.Dependency);

            job.Complete();
            cells.Dispose();
            cellOccupationState.Dispose();
        }
    }

    [BurstCompile]
    public partial struct UpdateJob : IJobEntity
    {
        [ReadOnly] public NativeArray<CellAspect> CellArray;
        [ReadOnly] public NativeArray<bool> CellOccupied;
        [ReadOnly] public GridComponent Grid;
        
        [BurstCompile]
        private void Execute(CellAspect cellAspect)
        {
            var position = cellAspect.Cell.ValueRO.Position;
            var newPos = new uint2(position.x, position.y + 1);
            var index = (int) MathUtilities.FlattenToIndex(newPos.x, newPos.y, Grid.Width);
            if (index < 0 || index >= CellArray.Length) return;
            if (CellOccupied[index]) return;
            cellAspect.Move(Grid, newPos);
        }
    }
}