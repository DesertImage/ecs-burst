using Unity.Mathematics;

namespace Game.Tween
{
    public struct TweenRotation
    {
        public float3 Target;
        public float Time;
        public float ElapsedTime;
        public EaseType Ease;
    }
}