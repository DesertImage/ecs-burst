using DesertImage.ECS;

namespace Game.Tween
{
    public struct TweenPositionTimeSystem : IExecuteSystem
    {
        public Matcher Matcher => MatcherBuilder.Create()
            .With<TweenPosition>()
            .Build();

        public void Execute(Entity entity, World world, float deltaTime)
        {
            ref var tween = ref entity.Get<TweenPosition>();

            tween.ElapsedTime += deltaTime;

            if (tween.ElapsedTime < tween.Time) return;

            entity.Remove<TweenPosition>();
        }
    }
}