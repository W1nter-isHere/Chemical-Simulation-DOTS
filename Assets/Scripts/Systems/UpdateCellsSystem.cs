using Aspects;
using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
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
                }.ScheduleParallel(state.Dependency)
                .Complete();

            new UpdateJob
                {
                    CellPositions = cellPositionsBuffer,
                    CellArray = cells,
                    Grid = grid
                }.ScheduleParallel(state.Dependency)
                .Complete();

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
            var i = CellPositions.IndexOf(new CellPosition {Position = cellAspect.Cell.ValueRO.Position});
            if (i < 0 || i >= CellArray.Length) return;
            CellArray[i] = cellAspect;
        }
    }

    [BurstCompile]
    public partial struct UpdateJob : IJobEntity
    {
        [WriteOnly] [NativeDisableParallelForRestriction]
        public DynamicBuffer<CellPosition> CellPositions;
        [ReadOnly] public NativeArray<CellAspect> CellArray;
        [ReadOnly] public GridComponent Grid;

        [BurstCompile]
        private void Execute(CellAspect cellAspect)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (cellAspect.Material.ValueRO.CellType)
            {
                case CellType.StationarySolid:
                    break;
                case CellType.FallingSolid:
                    var position = cellAspect.Cell.ValueRO.Position;
                    var newPos = new uint2(position.x, position.y - 1);

                    if (!Grid.ValidPosition(newPos)) return;

                    var newPositionsIndex = CellPositions.IndexOf(new CellPosition { Position = newPos });
                    var oldPositionIndex = CellPositions.IndexOf(new CellPosition { Position = position });
                    
                    // meaning no cells are there
                    if (newPositionsIndex < 0)
                    {
                        cellAspect.Move(Grid, newPos);
                        if (oldPositionIndex >= 0 && oldPositionIndex < CellPositions.Length)
                            CellPositions.RemoveAt(oldPositionIndex);
                        CellPositions.Add(new CellPosition { Position = newPos});
                    }

                    break;
                case CellType.Liquid:
                    break;
                case CellType.Gas:
                    break;
            }
        }
    }
}