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

        public void Execute(ref SystemsContext context)
        {
            foreach (var i in _group)
            {
                // entity.Replace<TestComponent>();
                _group.GetEntity(i).Remove<TestValueComponent>();
            }
        }
    }
}