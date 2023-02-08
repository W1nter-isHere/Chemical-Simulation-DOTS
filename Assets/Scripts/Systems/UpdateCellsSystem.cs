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
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var query = state.EntityManager.CreateEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<CellComponent, CellMaterialComponent>());
            if (query.IsEmpty) return;
            if (!SystemAPI.TryGetSingleton<GridComponent>(out var grid)) return;

            var entityCount = query.CalculateEntityCount();
            var cellPositionsBuffer = SystemAPI.GetSingletonBuffer<CellPosition>();
            var cells = new NativeArray<CellAspect>(entityCount, Allocator.TempJob);

            new FillNativeArraysJob
                {
                    CellPositions = cellPositionsBuffer,
                    CellArray = cells
                }
                .ScheduleParallel(state.Dependency)
                .Complete();

            var newPositions = new NativeList<CellPosition>(entityCount, Allocator.TempJob);

            new UpdateJob
                {
                    ResultingCellPositions = newPositions.AsParallelWriter(),
                    CellPositions = cellPositionsBuffer,
                    CellArray = cells,
                    Grid = grid
                }
                .Schedule(state.Dependency)
                .Complete();

            cellPositionsBuffer.Clear();
            cellPositionsBuffer.AddRange(newPositions.AsArray());

            newPositions.Dispose();
            cells.Dispose();
        }
    }

    [BurstCompile]
    public partial struct FillNativeArraysJob : IJobEntity
    {
        [NativeDisableParallelForRestriction] [WriteOnly]
        public NativeArray<CellAspect> CellArray;

        [ReadOnly] public DynamicBuffer<CellPosition> CellPositions;

        [BurstCompile]
        private void Execute(CellAspect cellAspect)
        {
            var i = CellPositions.IndexOf(cellAspect.Cell.ValueRO.Position);
            if (i < 0 || i >= CellArray.Length) return;
            CellArray[i] = cellAspect;
        }
    }

    [BurstCompile]
    public partial struct UpdateJob : IJobEntity
    {
        [WriteOnly] public NativeList<CellPosition>.ParallelWriter ResultingCellPositions;
        [ReadOnly] public DynamicBuffer<CellPosition> CellPositions;
        [ReadOnly] public NativeArray<CellAspect> CellArray;
        [ReadOnly] public GridComponent Grid;

        [BurstCompile]
        private void Execute(CellAspect cellAspect)
        {
            var currentPosition = cellAspect.Cell.ValueRO.Position;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (cellAspect.Material.ValueRO.CellType)
            {
                case CellType.StationarySolid:
                    StayInPlace(currentPosition);
                    break;
                case CellType.FallingSolid:
                    if (MoveIfCan(new uint2(currentPosition.x, currentPosition.y - 1), cellAspect)) return;
                    if (MoveIfCan(new uint2(currentPosition.x - 1, currentPosition.y - 1), cellAspect)) return;
                    if (MoveIfCan(new uint2(currentPosition.x + 1, currentPosition.y - 1), cellAspect)) return;
                    StayInPlace(currentPosition);
                    break;
                case CellType.Liquid:
                    StayInPlace(currentPosition);
                    break;
                case CellType.Gas:
                    StayInPlace(currentPosition);
                    break;
            }
        }

        [BurstCompile]
        private void StayInPlace(uint2 currentPosition)
        {
            ResultingCellPositions.AddNoResize(currentPosition);
        }

        [BurstCompile]
        private void MoveTo(uint2 position, CellAspect cellAspect)
        {
            cellAspect.Move(Grid, position);
            ResultingCellPositions.AddNoResize(position);
        }

        [BurstCompile]
        private bool MoveIfCan(uint2 direction, CellAspect cellAspect)
        {
            if (!CanMove(direction)) return false;
            MoveTo(direction, cellAspect);
            return true;
        }
        
        [BurstCompile]
        private bool CanMove(uint2 direction, uint magnitude = 1)
        {
            if (!Grid.ValidPosition(direction)) return false;
            return CellPositions.IndexOf(direction) == -1;
        }

        [BurstCompile]
        private bool CanMoveIncludeInfo(uint2 direction, out CellAspect cellAtPos, uint magnitude = 1)
        {
            if (!Grid.ValidPosition(direction))
            {
                cellAtPos = default;
                return false;
            }

            var index = CellPositions.IndexOf(direction);
            if (index != -1)
            {
                cellAtPos = default;
                return false;
            }

            cellAtPos = CellArray[index];
            return true;
        }
    }
}