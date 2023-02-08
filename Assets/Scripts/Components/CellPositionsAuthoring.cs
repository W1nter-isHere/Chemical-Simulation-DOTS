using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Components
{
    public class CellPositionsAuthoring : MonoBehaviour
    {
        private class CellPositionsBaker : Baker<CellPositionsAuthoring>
        {
            public override void Bake(CellPositionsAuthoring authoring)
            {
                AddBuffer<CellPosition>();
            }
        }
    }

    public struct CellPosition : IBufferElementData, IEquatable<CellPosition>
    {
        public uint2 Position;

        public static implicit operator CellPosition(uint2 position)
        {
            return new CellPosition { Position = position };
        } 
        
        public bool Equals(CellPosition other)
        {
            return Position.Equals(other.Position);
        }

        public override bool Equals(object obj)
        {
            return obj is CellPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }
}