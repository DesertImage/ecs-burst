namespace DesertImage.ECS
{
    public struct TestValueSecondSystem : IInitialize, IExecute
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<TestValueComponent>()
                .None<TestComponent>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var testValueComponents = _group.GetComponents<TestValueComponent>();
            foreach (var entityId in _group)
            {
                testValueComponents.Get(entityId).Value++;
            }
        }
    }
}