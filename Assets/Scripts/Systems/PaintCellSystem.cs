using Aspects;
using Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Systems
{
    public partial struct PaintCellSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }
        
        public void OnDestroy(ref SystemState state)
        {
        }
        
        public void OnUpdate(ref SystemState state)
        {
            #region Instantiate Newly Spawned Cells Data
            
            var grid = SystemAPI.GetSingleton<GridComponent>();
            
            var query = state.EntityManager.CreateEntityQuery(new EntityQueryBuilder(Allocator.Temp).WithAll<NewlyInstantiatedCellTag>());
            var newlyCreatedCells = query.ToEntityArray(Allocator.Temp);
            var newlyCreatedCellAspects = new NativeArray<CellAspect>(newlyCreatedCells.Length, Allocator.TempJob);
            
            for (var i = 0; i < newlyCreatedCells.Length; i++)
            {
                newlyCreatedCellAspects[i] = SystemAPI.GetAspectRW<CellAspect>(newlyCreatedCells[i]);
            }
            
            new SetupNewlyInstantiatedCellsJob
                {
                    Grid = grid,
                    CellAspects = newlyCreatedCellAspects
                }
                .Schedule(newlyCreatedCellAspects.Length, 16)
                .Complete();
            
            var entityCommandBuffer = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            entityCommandBuffer.RemoveComponent<NewlyInstantiatedCellTag>(newlyCreatedCells);
            
            newlyCreatedCells.Dispose();
            newlyCreatedCellAspects.Dispose();
            
            #endregion
            
            #region Spawn New Cells
            
            var cellPrefab = SystemAPI.GetSingleton<CellPrefabComponent>();
            var brush = SystemAPI.GetSingleton<BrushComponent>();
            var cellPositions = SystemAPI.GetSingletonBuffer<CellPosition>();
            
            foreach (var spawnCellQueue in SystemAPI.Query<DynamicBuffer<CellSpawnQueue>>())
            {
                if (spawnCellQueue.Length <= 0) continue;
                var positions = new NativeList<uint2>(Allocator.TempJob);
                var cellPositionsNativeArray = cellPositions.ToNativeArray(Allocator.Temp);

                new CalculatePositionsJob
                    {
                        Brush = brush,
                        Grid = grid,
                        Positions = positions,
                        SpawnQueues = spawnCellQueue,
                    }
                    .Schedule(spawnCellQueue.Length, 64)
                    .Complete();
            
                spawnCellQueue.Clear();
            
                var count = positions.Length;
                var entities = new NativeArray<Entity>(count, Allocator.Temp);
            
                entityCommandBuffer.Instantiate(cellPrefab.CellPrefab, entities);
                for (var i = 0; i < count; i++)
                {
                    var position = positions[i];
                    var cellPosition = new CellPosition { Position = position };

                    if (cellPositionsNativeArray.Contains(cellPosition)) return;
                    
                    var entity = entities[i];
                    cellPositions.Add(cellPosition);
                    
                    entityCommandBuffer.SetComponent(entity, new CellComponent
                    {
                        Position = position
                    });
                    entityCommandBuffer.AddComponent<NewlyInstantiatedCellTag>(entities);
                }

                cellPositionsNativeArray.Dispose();
                positions.Dispose();
                entities.Dispose();
            }
            
            #endregion
        }
    }

    public struct CalculatePositionsJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction] public NativeList<uint2> Positions;
        
        [ReadOnly] public GridComponent Grid;
        [ReadOnly] public BrushComponent Brush;
        [ReadOnly] public DynamicBuffer<CellSpawnQueue> SpawnQueues;

        public void Execute(int index)
        {
            var gridPosition = Grid.WorldToGrid(SpawnQueues[index].Position);
            if (!Grid.ValidPosition(gridPosition)) return;
            GetRoundedRectanglePositions(ref Positions, gridPosition.x, gridPosition.y, Brush.BrushSize);
        }

        private static void GetRoundedRectanglePositions(ref NativeList<uint2> positions, uint centerX, uint centerY, uint radius)
        {
            var rr = radius - 1;
            
            for (var r = 0; r < rr; r++) {
                for (var x = centerX - rr; x <= centerX + rr; x++) {
                    for (var y = centerY - rr; y <= centerY + rr; y++)
                    {
                        if (!(math.sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY)) <= r + 1) ||
                            !(math.sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY)) > r)) continue;
                        var value = new uint2(x, y);
                        if (positions.Contains(value)) continue;
                        positions.Add(value);
                    }
                }
            }

            var center = new uint2(centerX, centerY);
            if (positions.Contains(center)) return;
            positions.Add(center);
        }
    }

    public struct SetupNewlyInstantiatedCellsJob : IJobParallelFor
    {
        [ReadOnly] public GridComponent Grid;
        [ReadOnly] public NativeArray<CellAspect> CellAspects;
        
        public void Execute(int index)
        {
            var cellAspect = CellAspects[index];
            cellAspect.Move(Grid, cellAspect.Cell.ValueRO.Position);
        }
    }
}