﻿namespace DesertImage.ECS
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

        public void Execute(ref SystemsContext context)
        {
            // var testValueComponents = _group.GetComponents<TestValueComponent>();
            // for (var i = 0; i < testValueComponents.Length; i++)
            // {
            // testValueComponents.Get(i).Value++;
            // }

            foreach (var entity in _group)
            {
                entity.Get<TestValueComponent>().Value++;
            }
        }
    }
}