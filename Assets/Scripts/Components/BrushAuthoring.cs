using Unity.Entities;
using UnityEngine;

namespace Components
{
    public class BrushAuthoring : MonoBehaviour
    {
        public BrushType brushType;
        public uint brushSize = 1;
    }

    public class BrushBaker : Baker<BrushAuthoring>
    {
        public override void Bake(BrushAuthoring authoring)
        {
            AddComponent(new BrushComponent
            {
                BrushType = authoring.brushType,
                BrushSize = authoring.brushSize
            });
        }
    }

    public struct BrushComponent : IComponentData
    {
        public BrushType BrushType;
        public uint BrushSize;
    }

    public enum BrushType
    {
        Circular,
        Rectangular
    }
}