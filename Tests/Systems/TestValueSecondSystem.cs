namespace DesertImage.ECS
{
    public struct TestValueSecondSystem : IExecuteSystem
    {
        public Matcher Matcher => MatcherBuilder.Create()
            .With<TestValueComponent>()
            .None<TestComponent>()
            .Build();

        public void Execute(Entity entity, float deltaTime)
        {
            ref var testValueComponent = ref entity.Get<TestValueComponent>();
            testValueComponent.Value++;
        }
    }
}