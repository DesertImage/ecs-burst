using Unity.Mathematics;

namespace Game.Input
{
    public struct MousePosition
    {
        public float3 Value;
        public float3 WorldPosition;

        public float ZOffset;
    }
}