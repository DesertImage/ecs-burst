namespace DesertImage.ECS
{
    public struct TestValueThirdSystem : IExecuteSystem
    {
        public Matcher Matcher => MatcherBuilder.Create()
            .With<TestValueComponent>()
            .None<TestComponent>()
            .Build();

        public void Execute(Entity entity, World world, float deltaTime)
        {
            entity.Replace<TestComponent>();
            entity.Remove<TestValueComponent>();
        }
    }
}