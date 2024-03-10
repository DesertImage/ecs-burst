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
                    Start = entity.Read<Position>().Value,
                    End = target,
                    Time = time,
                    Ease = easeType
                }
            );
        }

        public static void TweenLocalPosition(this ref Entity entity, float3 target, float time,
            EaseType easeType = EaseType.OutExpo)
        {
            entity.Replace
            (
                new TweenLocalPosition
                {
                    Start = entity.Read<LocalPosition>().Value,
                    End = target,
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
                    Start = entity.Read<Rotation>().Value,
                    End = target,
                    Time = time,
                    Ease = easeType
                }
            );
        }

        public static void TweenLocalRotation(this ref Entity entity, float3 target, float time,
            EaseType easeType = EaseType.OutExpo)
        {
            entity.Replace
            (
                new TweenLocalRotation
                {
                    Start = entity.Read<LocalRotation>().Value,
                    End = target,
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
                    Start = entity.Read<Scale>().Value,
                    End = target,
                    Time = time,
                    Ease = easeType
                }
            );
        }
    }
}