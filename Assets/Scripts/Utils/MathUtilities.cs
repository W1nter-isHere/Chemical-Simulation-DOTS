using Unity.Burst;
using Unity.Mathematics;

namespace Utils
{
    public static class MathUtilities
    {
        [BurstCompile]
        public static uint FlattenToIndex(uint x, uint y, uint width)
        {
            return y * width + x;
        }
        
        [BurstCompile]
        public static uint2 IndexTo2D(uint index, uint width)
        {
            var x = index % width;
            var y = index / width;
            return new uint2(x, y);
        }
    }
}