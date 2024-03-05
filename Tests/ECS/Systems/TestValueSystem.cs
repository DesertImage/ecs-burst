namespace DesertImage.ECS
{
    public struct TestValueSystem : IInitSystem, ICalculateSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<TestValueComponent>()
                .None<TestComponent>()
                .Find();
        }

        public unsafe void Execute(SystemsContext* context)
        {
            var testValueComponents = _group.GetComponents<TestValueComponent>();
            for (var i = 0; i < testValueComponents.Length; i++)
            {
                testValueComponents.Get(i).Value++;
            }
        }
    }
}