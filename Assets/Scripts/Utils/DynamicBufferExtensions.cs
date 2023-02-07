using System;
using Unity.Burst;
using Unity.Entities;

namespace Utils
{
    public static class DynamicBufferExtensions
    {
        [BurstCompile]
        public static int IndexOf<T>(this DynamicBuffer<T> dynamicBuffer, T obj) where T : unmanaged, IEquatable<T>
        {
            var i = -1;

            for (var j = 0; j < dynamicBuffer.Length; j++)
            {
                if (dynamicBuffer[j].Equals(obj)) i = j;
            }

            return i;
        }

        [BurstCompile]
        public static bool Contains<T>(this DynamicBuffer<T> dynamicBuffer, T obj) where T : unmanaged, IEquatable<T>
        {
            foreach (var element in dynamicBuffer)
            {
                if (element.Equals(obj)) return true;
            }

            return false;
        }
    }
}