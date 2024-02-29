using DesertImage.ECS;
using Unity.Mathematics;

namespace Game.Tween
{
    public struct TweenRotationSystem : IExecuteSystem
    {
        public Matcher Matcher => MatcherBuilder.Create()
            .With<Rotation>()
            .With<TweenRotation>()
            .Build();

        public void Execute(Entity entity, World world, float deltaTime)
        {
            ref var tween = ref entity.Get<TweenRotation>();
            ref var rotation = ref entity.Get<Rotation>();

            rotation.Value = math.lerp
            (
                rotation.Value,
                tween.Target,
                Easing.GetEase(tween.Ease, tween.ElapsedTime / tween.Time)
            );
        }
    }
}