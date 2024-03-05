namespace DesertImage.ECS
{
    public struct TestValueRemoveSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world).With<TestValueComponent>().None<TestComponent>().Build();
        }

        public unsafe void Execute(SystemsContext* context)
        {
            foreach (var entity in _group)
            {
                var testValueComponent = entity.Read<TestValueComponent>();
                if (testValueComponent.Value < 2) return;

                entity.Remove<TestValueComponent>();
            }
        }
    }
}