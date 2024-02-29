namespace DesertImage.ECS
{
    public struct TestValueSecondSystem : ICalculateSystem
    {
        public Matcher Matcher => MatcherBuilder.Create()
            .With<TestValueComponent>()
            .None<TestComponent>()
            .Build();

        public void Execute(Entity entity, World world, float deltaTime)
        {
            ref var testValueComponent = ref entity.Get<TestValueComponent>();
            testValueComponent.Value++;
        }
    }
}