using DesertImage.ECS;
using UnityEngine;

namespace Game.Tween
{
    public struct TweenRotationTimeSystem : IExecuteSystem
    {
        public Matcher Matcher => MatcherBuilder.Create()
            .With<TweenRotation>()
            .Build();

        public void Execute(Entity entity, World world, float deltaTime)
        {
            ref var tween = ref entity.Get<TweenRotation>();

            tween.ElapsedTime += deltaTime;

            if (tween.ElapsedTime < tween.Time) return;

            entity.Remove<TweenRotation>();
        }
    }
}