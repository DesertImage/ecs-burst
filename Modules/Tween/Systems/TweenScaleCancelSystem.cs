using DesertImage.ECS;

namespace Game.Tween
{
    public struct TweenScaleCancelSystem : IExecuteSystem
    {
        public Matcher Matcher => MatcherBuilder.Create()
            .With<TweenScale>()
            .AnyOf<TweenCancelAll, TweenScaleCancel>()
            .Build();

        public void Execute(Entity entity, World world, float deltaTime) => entity.Remove<TweenScale>();
    }
}