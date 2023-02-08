using Aspects;
using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

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
            var query = state.EntityManager.CreateEntityQuery(new EntityQueryBuilder(Allocator.Temp).WithAll<CellComponent, CellMaterialComponent>());
            if (query.IsEmpty) return;
            if (!SystemAPI.TryGetSingleton<GridComponent>(out var grid)) return;

            var cellPositionsGrid = SystemAPI.GetSingleton<CellPositionsComponent>().Grid;
            var toRemovePositions = new NativeList<uint2>(query.CalculateEntityCount(), Allocator.TempJob);
            var toAddPositions = new NativeParallelHashMap<uint2, CellAspect>(query.CalculateEntityCount(), Allocator.TempJob);

            new UpdateJob
                {
                    ToRemoveCellPositions = toRemovePositions.AsParallelWriter(),
                    ToAddCellPositions = toAddPositions.AsParallelWriter(),
                    CellPositionsGrid = cellPositionsGrid, 
                    Grid = grid
                }
                .Schedule(state.Dependency)
                .Complete();

            foreach (var toRemove in toRemovePositions)
            {
                cellPositionsGrid.Remove(toRemove);
            }

            foreach (var pair in toAddPositions)
            {
                cellPositionsGrid.TryAdd(pair.Key, pair.Value);
            }

            toRemovePositions.Dispose();
            toAddPositions.Dispose();
        }
    }

    [BurstCompile]
    public partial struct UpdateJob : IJobEntity
    {
        [WriteOnly] public NativeList<uint2>.ParallelWriter ToRemoveCellPositions;
        [WriteOnly] public NativeParallelHashMap<uint2, CellAspect>.ParallelWriter ToAddCellPositions;
        [ReadOnly] public NativeParallelHashMap<uint2, CellAspect> CellPositionsGrid;
        [ReadOnly] public GridComponent Grid;

        [BurstCompile]
        private void Execute(CellAspect cellAspect)
        {
            var currentPosition = cellAspect.Cell.ValueRO.Position;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (cellAspect.Material.ValueRO.CellType)
            {
                case CellType.StationarySolid:
                    break;
                case CellType.FallingSolid:
                    if (MoveIfCan(currentPosition, new uint2(currentPosition.x, currentPosition.y - 1), cellAspect)) return;
                    if (MoveIfCan(currentPosition, new uint2(currentPosition.x - 1, currentPosition.y - 1), cellAspect)) return;
                    MoveIfCan(currentPosition, new uint2(currentPosition.x + 1, currentPosition.y - 1), cellAspect);
                    break;
                case CellType.Liquid:
                    break;
                case CellType.Gas:
                    break;
            }
        }

        [BurstCompile]
        private void MoveTo(uint2 origin, uint2 destination, CellAspect cellAspect)
        {
            cellAspect.Move(Grid, destination);
            ToRemoveCellPositions.AddNoResize(origin);
            ToAddCellPositions.TryAdd(destination, cellAspect);
        }

        [BurstCompile]
        private bool MoveIfCan(uint2 origin, uint2 direction, CellAspect cellAspect)
        {
            if (!CanMove(direction)) return false;
            MoveTo(origin, direction, cellAspect);
            return true;
        }
        
        [BurstCompile]
        private bool CanMove(uint2 direction, uint magnitude = 1)
        {
            if (!Grid.ValidPosition(direction)) return false;
            return !CellPositionsGrid.ContainsKey(direction);
        }

        [BurstCompile]
        private bool CanMoveIncludeInfo(uint2 direction, out CellAspect cellAtPos, uint magnitude = 1)
        {
            if (!Grid.ValidPosition(direction))
            {
                cellAtPos = default;
                return false;
            }

            if (!CellPositionsGrid.ContainsKey(direction))
            {
                cellAtPos = default;
                return false;
            }

            cellAtPos = CellPositionsGrid[direction];
            return true;
        }
    }
}