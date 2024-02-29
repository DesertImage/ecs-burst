using DesertImage.ECS;
using Unity.Mathematics;

namespace Game.Tween
{
    public struct TweenPositionSystem : IExecuteSystem
    {
        public Matcher Matcher => MatcherBuilder.Create()
            .With<Position>()
            .With<TweenPosition>()
            .Build();

        public void Execute(Entity entity, World world, float deltaTime)
        {
            ref var tween = ref entity.Get<TweenPosition>();
            ref var position = ref entity.Get<Position>();

            position.Value = math.lerp
            (
                position.Value,
                tween.Target,
                Easing.GetEase(tween.Ease, tween.ElapsedTime / tween.Time)
            );
        }
    }
}