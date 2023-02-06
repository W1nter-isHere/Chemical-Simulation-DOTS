using Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Systems
{
    public partial class SetupGridSystem : SystemBase
    {
        private Camera _camera;
        private float _previousOrthographicSize;
        
        protected override void OnCreate()
        {
            _camera = Camera.main;
        }

        protected override void OnUpdate()
        {
            var changed = math.abs(_camera.orthographicSize - _previousOrthographicSize) > 0.01;
            _previousOrthographicSize = _camera.orthographicSize;
            if (!changed) return;
            var grid = SystemAPI.GetSingletonRW<GridComponent>();
            
            var aspect = (float)Screen.width / Screen.height;
            var worldHeight = _camera.orthographicSize * 2;
            var worldWidth = worldHeight * aspect;
            var btmLeft = new float2(-worldWidth / 2, -worldHeight / 2);
            
            grid.ValueRW.Width = (uint) math.ceil(worldWidth / grid.ValueRO.CellWidth);
            grid.ValueRW.Height = (uint) math.ceil(worldHeight / grid.ValueRO.CellHeight);
            grid.ValueRW.OffsetX = btmLeft.x;
            grid.ValueRW.OffsetY = btmLeft.y;
        }
    }
}