namespace DesertImage.ECS
{
    public struct TestObjectReferenceSystem : IExecuteSystem
    {
        public Matcher Matcher => MatcherBuilder.Create()
            .With<TestReferenceComponent>()
            .Build();

        public void Execute(Entity entity, World world, float deltaTime)
        {
            ref var testValueComponent = ref entity.Get<TestReferenceComponent>();
            testValueComponent.Rigidbody.Value.mass = 1234;
        }
    }
}