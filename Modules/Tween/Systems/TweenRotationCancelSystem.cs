using DesertImage.ECS;

namespace Game.Tween
{
    public struct TweenRotationCancelSystem : IExecuteSystem
    {
        public Matcher Matcher => MatcherBuilder.Create()
            .With<TweenRotation>()
            .AnyOf<TweenCancelAll, TweenRotationCancel>()
            .Build();

        public void Execute(Entity entity, World world, float deltaTime) => entity.Remove<TweenRotation>();
    }
}