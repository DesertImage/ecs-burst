namespace DesertImage.ECS
{
    public struct TestComplexSystem : IInitialize, IExecute
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<TestComplexComponent>()
                .With<TestTag>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var components = _group.GetComponents<TestComplexComponent>();
            foreach (var i in _group)
            {
                ref var component = ref components.Get(i);

                component.ElapsedTime += context.DeltaTime;

                if (component.ElapsedTime < component.TargetTime) continue;

                _group.GetEntity(i).Remove<TestComplexComponent>();
            }
        }
    }
}