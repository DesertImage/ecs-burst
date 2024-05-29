namespace DesertImage.ECS
{
    public struct TestObjectReferenceSystem : IInitialize, IExecute
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world).With<TestReferenceComponent>().Find();
        }

        public void Execute(ref SystemsContext context)
        {
            foreach (var i in _group)
            {
                ref var testValueComponent = ref _group.GetEntity(i).Get<TestReferenceComponent>();
                testValueComponent.Rigidbody.Value.mass = 1234;
            }
        }
    }
}