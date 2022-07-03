using System;

namespace DesertImage.ECS.Tests
{
    [Serializable]
    public class TestComponent : Component<TestComponent>
    {
        public override ushort Id => 0;

        public override void ReturnToPool()
        {
            base.ReturnToPool();

            ComponentsTool.ReturnToPool(this);
        }
    }

    public class TestComponentWrapper : ComponentWrapper<TestComponent>
    {
    }

    public static partial class ComponentsExtension
    {
        public static TestComponent GetTestComponent(this IComponentHolder componentHolder)
        {
            return componentHolder.Get<TestComponent>(0);
        }

        public static bool HasTestComponent(this IComponentHolder componentHolder)
        {
            return componentHolder.HasComponent(0);
        }

        public static TestComponent AddTestComponent(this IComponentHolder componentHolder)
        {
            return componentHolder.Add<TestComponent>();
        }

        public static void RemoveTestComponent(this IComponentHolder componentHolder)
        {
            componentHolder.Remove(0);
        }
    }
}