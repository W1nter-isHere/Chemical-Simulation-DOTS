using Aspects;
using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Systems
{
    public partial class PaintCellSystem : SystemBase
    {
        private Camera _camera;

        protected override void OnCreate()
        {
            _camera = Camera.main;
        }

        protected override void OnUpdate()
        {
            var grid = SystemAPI.GetSingleton<GridComponent>();

            var query = EntityManager.CreateEntityQuery(typeof(NewlyInstantiatedCellTag));
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
                .Schedule(newlyCreatedCellAspects.Length, 32)
                .Complete();

            var entityCommandBuffer = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(World.Unmanaged);

            foreach (var entity in newlyCreatedCells)
            {
                entityCommandBuffer.RemoveComponent<NewlyInstantiatedCellTag>(entity);
            }
            
            newlyCreatedCells.Dispose();
            newlyCreatedCellAspects.Dispose();
            
            if (!Input.GetMouseButton(0)) return;
            
            var position = _camera.ScreenToWorldPoint(Input.mousePosition);
            var mousePosition = new float3(position.x, position.y, 0);

            var cellPrefab = SystemAPI.GetSingleton<CellPrefabComponent>();
            var gridPosition = grid.WorldToGrid(mousePosition);
            if (!grid.ValidPosition(gridPosition)) return;

            var positions = new NativeList<uint2>(Allocator.TempJob);
            var brush = SystemAPI.GetSingleton<BrushComponent>();
            
            positions.Add(gridPosition);
            new GetPositionsJob
            {
                Center = gridPosition,
                Positions = positions,
                Radius = brush.BrushSize
            }
                .Schedule((int) brush.BrushSize, 32)
                .Complete();
            
            var count = positions.Length;
            var entities = new NativeArray<Entity>(count, Allocator.Temp);
            
            entityCommandBuffer.Instantiate(cellPrefab.CellPrefab, entities);
            for (var i = 0; i < count; i++)
            {
                var entity = entities[i];
                entityCommandBuffer.SetComponent(entity, new CellComponent
                {
                    Position = positions[i]
                });
                entityCommandBuffer.AddComponent<NewlyInstantiatedCellTag>(entities);
            }

            positions.Dispose();
            entities.Dispose();
        }
    }

    [BurstCompile]
    public struct GetPositionsJob : IJobParallelFor
    {
        [ReadOnly] public uint Radius;
        [ReadOnly] public uint2 Center;
        [WriteOnly] [NativeDisableParallelForRestriction] 
        public NativeList<uint2> Positions;

        [BurstCompile]
        public void Execute(int r)
        {
            var rr = Radius - 1;
            
            for (var x = Center.x - rr; x <= Center.x + rr; x++) {
                for (var y = Center.y - rr; y <= Center.y + rr; y++) {
                    if (Mathf.Sqrt((x - Center.x) * (x - Center.x) + (y - Center.y) * (y - Center.y)) <= r + 1 && Mathf.Sqrt((x - Center.x) * (x - Center.x) + (y - Center.y) * (y - Center.y)) > r) {
                        Positions.Add(new uint2(x, y));
                    }
                }
            }
        }
    }

    [BurstCompile]
    public struct SetupNewlyInstantiatedCellsJob : IJobParallelFor
    {
        [ReadOnly] public GridComponent Grid;
        [ReadOnly] public NativeArray<CellAspect> CellAspects;

        [BurstCompile]
        public void Execute(int index)
        {
            var cellAspect = CellAspects[index];
            cellAspect.Move(Grid, cellAspect.Cell.ValueRO.Position);
        }
    }
}