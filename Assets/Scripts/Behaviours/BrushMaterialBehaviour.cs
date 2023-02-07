using System;
using System.Linq;
using Components;
using TMPro;
using Unity.Entities;
using UnityEngine;

namespace Behaviours
{
    public class BrushMaterialBehaviour : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown cellTypeDropdown;
        [SerializeField] private TMP_Dropdown brushSizeDropdown;

        private EntityQuery _brush;
        
        private void Start()
        {
            cellTypeDropdown.AddOptions(Enum.GetValues(typeof(CellType)).Cast<CellType>().Select(c => c.ToString()).ToList());
            cellTypeDropdown.onValueChanged.AddListener(TypeChanged);
            brushSizeDropdown.onValueChanged.AddListener(BrushSizeChanged);
            _brush = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(BrushComponent));
        }

        private void OnDestroy()
        {
            cellTypeDropdown.onValueChanged.RemoveListener(TypeChanged);
            brushSizeDropdown.onValueChanged.RemoveListener(BrushSizeChanged);
        }

        private void TypeChanged(int option)
        {
            _brush.GetSingletonRW<BrushComponent>().ValueRW.CellType = (CellType)Enum.GetValues(typeof(CellType)).GetValue(option);
        }

        public void BrushSizeChanged(int option)
        {
            _brush.GetSingletonRW<BrushComponent>().ValueRW.BrushSize = (uint)(option + 1);
        } 
    }
}