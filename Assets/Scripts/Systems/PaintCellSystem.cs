using System.Diagnostics;
using Aspects;
using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

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

            var positions = new NativeList<uint2>(Allocator.Temp);
            var brush = SystemAPI.GetSingleton<BrushComponent>();
            GetRoundedRectanglePositions(ref positions, gridPosition, brush.BrushSize);

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

        private static void GetRoundedRectanglePositions(ref NativeList<uint2> positions, uint2 center, uint radius)
        {
            var rr = radius - 1;
            
            for (var r = 0; r < rr; r++) {
                for (var x = center.x - rr; x <= center.x + rr; x++) {
                    for (var y = center.y - rr; y <= center.y + rr; y++) {
                        if (Mathf.Sqrt((x - center.x) * (x - center.x) + (y - center.y) * (y - center.y)) <= r + 1 && Mathf.Sqrt((x - center.x) * (x - center.x) + (y - center.y) * (y - center.y)) > r) {
                            positions.Add(new uint2(x, y));
                        }
                    }
                }
            }
            
            positions.Add(center);
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