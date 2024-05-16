namespace DesertImage.ECS
{
    public struct TestValueSystem : IInitSystem, IExecuteSystem
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
            foreach (var i in _group)
            {
                testValueComponents.Get(i).Value++;
            }
        }
    }
}