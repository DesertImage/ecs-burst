using DesertImage.ECS;

namespace Game.Tween
{
    public struct TweenScaleTimeSystem : IExecuteSystem
    {
        public Matcher Matcher => MatcherBuilder.Create()
            .With<TweenScale>()
            .Build();

        public void Execute(Entity entity, World world, float deltaTime)
        {
            ref var tween = ref entity.Get<TweenScale>();

            tween.ElapsedTime += deltaTime;

            if (tween.ElapsedTime < tween.Time) return;

            entity.Remove<TweenScale>();
        }
    }
}