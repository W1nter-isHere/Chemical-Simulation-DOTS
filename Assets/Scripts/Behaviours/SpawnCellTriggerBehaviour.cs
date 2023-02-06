using Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Behaviours
{
    public class SpawnCellTriggerBehaviour : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        
        private World _world;
        private Entity _queueEntity;

        private void Start()
        {
            _world = World.DefaultGameObjectInjectionWorld;
        }

        private void Update()
        {
            if (!Input.GetMouseButton(0)) return;
            if (_world.IsCreated && !_world.EntityManager.Exists(_queueEntity))
            {
                _queueEntity = _world.EntityManager.CreateEntity();
                _world.EntityManager.AddBuffer<CellSpawnQueue>(_queueEntity);
            }
            
            float3 position = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            _world.EntityManager.GetBuffer<CellSpawnQueue>(_queueEntity)
                .Add(new CellSpawnQueue { Position = position });
        }
    }
}