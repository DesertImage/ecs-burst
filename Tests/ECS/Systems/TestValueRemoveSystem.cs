using UnityEngine;

namespace DesertImage.ECS
{
    public struct TestValueRemoveSystem : IExecuteSystem
    {
        public Matcher Matcher => MatcherBuilder.Create()
            .With<TestValueComponent>()
            .None<TestComponent>()
            .Build();

        public void Execute(Entity entity, World world, float deltaTime)
        {
            Debug.Log($"Execute {this}. Entity: {entity.Id}");

            var testValueComponent = entity.Read<TestValueComponent>();
            if (testValueComponent.Value < 2) return;

            entity.Remove<TestValueComponent>();
        }
    }
}