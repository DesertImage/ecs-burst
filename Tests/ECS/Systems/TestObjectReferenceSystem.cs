﻿namespace DesertImage.ECS
{
    public struct TestObjectReferenceSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world).With<TestReferenceComponent>().Find();
        }

        public void Execute(ref SystemsContext context)
        {
            foreach (var entity in _group)
            {
                ref var testValueComponent = ref entity.Get<TestReferenceComponent>();
                testValueComponent.Rigidbody.Value.mass = 1234;
            }
        }
    }
}