using Behaviours;
using Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Systems
{
    public partial class SetupGridSystem : SystemBase
    {
        private bool _initialized;
        private float _previousOrthographicSize;

        protected override void OnUpdate()
        {
            var camera = CameraZoomBehaviour.Instance;
            var changed = math.abs(camera.OrthographicSize - _previousOrthographicSize) > 0.01;
            _previousOrthographicSize = camera.OrthographicSize;
            if (!changed && _initialized) return;
            if (!SystemAPI.TryGetSingletonRW<GridComponent>(out var grid)) return;

            var aspect = (float)Screen.width / Screen.height;
            var worldHeight = camera.OrthographicSize * 2;
            var worldWidth = worldHeight * aspect;
            var btmLeft = new float2(-worldWidth / 2, -worldHeight / 2);
            
            grid.ValueRW.Width = (uint) math.ceil(worldWidth / grid.ValueRO.CellWidth);
            grid.ValueRW.Height = (uint) math.ceil(worldHeight / grid.ValueRO.CellHeight);
            grid.ValueRW.OffsetX = btmLeft.x;
            grid.ValueRW.OffsetY = btmLeft.y;

            _initialized = true;
        }
    }
}