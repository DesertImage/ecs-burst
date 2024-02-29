using UnityEngine;

namespace DesertImage.ECS
{
    public struct TestValueSystem : ICalculateSystem
    {
        public Matcher Matcher => MatcherBuilder.Create()
            .With<TestValueComponent>()
            .None<TestComponent>()
            .Build();

        public void Execute(Entity entity, World world, float deltaTime)
        {
            if (!entity.Has<TestValueComponent>())
            {
                Debug.Log("WRONG");
                return;
            }

            ref var testValueComponent = ref entity.Get<TestValueComponent>();
            testValueComponent.Value++;
        }
    }
}