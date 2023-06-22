namespace DesertImage.ECS
{
    public sealed class TestValueSystem : ExecuteSystem
    {
        public override Matcher Matcher =>
            MatcherBuilder.Create().With<TestValueComponent>().None<TestComponent>().Build();

        public override void Execute(Entity entity, float deltaTime)
        {
            entity.Get<TestValueComponent>().Value++;
        }
    }
}