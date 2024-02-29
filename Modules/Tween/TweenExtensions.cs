using DesertImage.ECS;
using Unity.Mathematics;

namespace Game.Tween
{
    public static class TweenExtensions
    {
        public static void TweenPosition(this ref Entity entity, float3 target, float time,
            EaseType easeType = EaseType.OutExpo)
        {
            entity.Replace
            (
                new TweenPosition
                {
                    Target = target,
                    Time = time,
                    Ease = easeType
                }
            );
        }

        public static void TweenRotation(this ref Entity entity, float3 target, float time,
            EaseType easeType = EaseType.OutExpo)
        {
            entity.Replace
            (
                new TweenRotation
                {
                    Target = target,
                    Time = time,
                    Ease = easeType
                }
            );
        }

        public static void TweenScale(this ref Entity entity, float3 target, float time,
            EaseType easeType = EaseType.OutExpo)
        {
            entity.Replace
            (
                new TweenScale
                {
                    Target = target,
                    Time = time,
                    Ease = easeType
                }
            );
        }
    }
}