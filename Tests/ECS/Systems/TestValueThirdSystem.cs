namespace DesertImage.ECS
{
    public struct TestValueThirdSystem : IInitSystem, IExecuteSystem
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
            foreach (var entity in _group)
            {
                // entity.Replace<TestComponent>();
                entity.Remove<TestValueComponent>();
            }
        }
    }
}