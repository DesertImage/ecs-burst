using Unity.Mathematics;

namespace Game.Tween
{
    public struct TweenScale
    {
        public float3 Target;
        public float Time;
        public float ElapsedTime;
        public EaseType Ease;
    }
}