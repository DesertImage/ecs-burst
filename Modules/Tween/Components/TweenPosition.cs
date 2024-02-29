using Unity.Mathematics;

namespace Game.Tween
{
    public struct TweenPosition
    {
        public float3 Target;
        public float Time;
        public float ElapsedTime;
        public EaseType Ease;
    }
}