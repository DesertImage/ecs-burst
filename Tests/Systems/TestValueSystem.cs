using UnityEngine;

namespace DesertImage.ECS
{
    public struct TestValueSystem : IExecuteSystem
    {
        public Matcher Matcher => MatcherBuilder.Create()
            .With<TestValueComponent>()
            .None<TestComponent>()
            .Build();

        public void Execute(Entity entity, World world, float deltaTime)
        {
            ref var testValueComponent = ref entity.Get<TestValueComponent>();
            testValueComponent.Value++;

            Debug.Log($"EXECUTE {testValueComponent.Value}");
        }
    }
}