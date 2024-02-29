using DesertImage.ECS;

namespace Game.Tween
{
    public struct TweenPositionCancelSystem : IExecuteSystem
    {
        public Matcher Matcher => MatcherBuilder.Create()
            .With<TweenPosition>()
            .AnyOf<TweenCancelAll, TweenPositionCancel>()
            .Build();

        public void Execute(Entity entity, World world, float deltaTime) => entity.Remove<TweenPosition>();
    }
}