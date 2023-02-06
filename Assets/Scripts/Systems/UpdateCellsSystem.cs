using Aspects;
using Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Systems
{
    public partial struct UpdateCellsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }
        
        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var query = state.EntityManager.CreateEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<CellComponent, CellMaterialComponent>());
            if (query.IsEmpty) return;

            var grid = SystemAPI.GetSingleton<GridComponent>();
            var entityCount = query.CalculateEntityCount();

            var positions = SystemAPI.GetSingletonBuffer<CellPosition>(true).ToNativeArray(Allocator.TempJob);
            var cells = new NativeArray<CellAspect>(entityCount, Allocator.TempJob);

            new FillNativeArraysJob
                {
                    CellPositions = positions,
                    CellArray = cells
                }.ScheduleParallel(state.Dependency)
                .Complete();

            new UpdateJob
                {
                    CellPositions = positions,
                    CellArray = cells,
                    Grid = grid
                }.ScheduleParallel(state.Dependency)
                .Complete();

            positions.Dispose();
            cells.Dispose();
        }
    }

    public partial struct FillNativeArraysJob : IJobEntity
    {
        [NativeDisableParallelForRestriction] [WriteOnly]
        public NativeArray<CellAspect> CellArray;
        
        [ReadOnly] public NativeArray<CellPosition> CellPositions;

        private void Execute(CellAspect cellAspect)
        {
            var i = CellPositions.IndexOf(new CellPosition {Position = cellAspect.Cell.ValueRO.Position});
            if (i < 0 || i >= CellArray.Length) return;
            CellArray[i] = cellAspect;
        }
    }

    public partial struct UpdateJob : IJobEntity
    {
        [ReadOnly] public NativeArray<CellPosition> CellPositions;
        [ReadOnly] public NativeArray<CellAspect> CellArray;
        [ReadOnly] public GridComponent Grid;

        private void Execute(CellAspect cellAspect)
        {
            switch (cellAspect.Material.ValueRO.CellType)
            {
                case CellType.StationarySolid:
                    break;
                case CellType.FallingSolid:
                    var position = cellAspect.Cell.ValueRO.Position;
                    var newPos = new uint2(position.x, position.y - 1);
                    var index = CellPositions.IndexOf(new CellPosition {Position = newPos});
                    if (index < 0 || index >= CellArray.Length) return;
                    cellAspect.Move(Grid, newPos);
                    break;
                case CellType.Liquid:
                    break;
                case CellType.Gas:
                    break;
            }
        }
    }
}