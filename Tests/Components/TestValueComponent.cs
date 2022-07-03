using System;

namespace DesertImage.ECS.Tests
{
    [Serializable]
    public class TestValueComponent : Component<TestValueComponent>
    {
        public override ushort Id => 1;

        public int Value;

        public override void ReturnToPool()
        {
            base.ReturnToPool();

            ComponentsTool.ReturnToPool(this);

            Value = default;
        }

        public override Component<TestValueComponent> CopyTo(Component<TestValueComponent> component)
        {
            var targetComponent = component as TestValueComponent;

            targetComponent.Value = Value;

            return component;
        }
    }

    public class TestValueComponentWrapper : ComponentWrapper<TestValueComponent>
    {
    }

    public static partial class ComponentsExtension
    {
        public static TestValueComponent GetTestValueComponent(this IComponentHolder componentHolder)
        {
            return componentHolder.Get<TestValueComponent>(1);
        }

        public static bool HasTestValueComponent(this IComponentHolder componentHolder)
        {
            return componentHolder.HasComponent(1);
        }

        public static int GetTestValueComponentValue(this IComponentHolder componentHolder)
        {
            var component = componentHolder.Get<TestValueComponent>(1);

            return component.Value;
        }

        public static void SetTestValueComponentValue(this IComponentHolder componentHolder, int value)
        {
            var component = componentHolder.Get<TestValueComponent>(1);

            var newInstance = ComponentsTool.GetInstanceFromPool<TestValueComponent>();

            component.CopyTo(newInstance);
            newInstance.Value = value;

            component.PreUpdated(newInstance);

            newInstance.ReturnToPool();

            component.Value = value;

            component.Updated();
        }

        public static TestValueComponent AddTestValueComponent(this IComponentHolder componentHolder, int value = default)
        {
            var component = ComponentsTool.GetInstanceFromPool<TestValueComponent>();

            component.Value = value;

            componentHolder.Add(component);

            return component;
        }

        public static void RemoveTestValueComponent(this IComponentHolder componentHolder)
        {
            componentHolder.Remove(1);
        }
    }
}