using DesertImage.ECS;
using Unity.Mathematics;

namespace Game.Tween
{
    public struct TweenScaleSystem : IExecuteSystem
    {
        public Matcher Matcher => MatcherBuilder.Create()
            .With<Scale>()
            .With<TweenScale>()
            .Build();

        public void Execute(Entity entity, World world, float deltaTime)
        {
            ref var tween = ref entity.Get<TweenScale>();
            ref var scale = ref entity.Get<Scale>();

            scale.Value = math.lerp
            (
                scale.Value,
                tween.Target,
                Easing.GetEase(tween.Ease, tween.ElapsedTime / tween.Time)
            );
        }
    }
}