using NUnit.Framework;

namespace DesertImage.ECS
{
    public class ComponentsTests
    {
        [Test]
        public void AddRemove()
        {
            var world = Worlds.Create();

            var entity = world.GetNewEntity();

            var firstResult = entity.Has<TestComponent>();

            entity.Replace(new TestComponent());

            var secondResult = entity.Has<TestComponent>();

            entity.Remove<TestComponent>();

            var thirdResult = entity.Has<TestComponent>();

            world.Dispose();

            Assert.IsFalse(firstResult);
            Assert.IsTrue(secondResult);
            Assert.IsFalse(thirdResult);
        }

        [Test]
        public void ChangeValue()
        {
            var world = Worlds.Create();

            var entity = world.GetNewEntity();

            entity.Replace(new TestValueComponent { Value = 2 });

            Assert.IsTrue(entity.Has<TestValueComponent>());

            var component = entity.Get<TestValueComponent>();
            component.Value = 5;

            var firstResult = entity.Get<TestValueComponent>().Value;

            ref var refComponent = ref entity.Get<TestValueComponent>();
            refComponent.Value = 5;

            var secondResult = entity.Get<TestValueComponent>().Value;

            world.Dispose();

            Assert.AreNotEqual(5, firstResult);
            Assert.AreEqual(5, secondResult);
        }

        [Test]
        public void ComponentsAfterPool()
        {
            var world = Worlds.Create();

            var entity = world.GetNewEntity();

            entity.Replace(new TestComponent());

            entity.Destroy();

            entity = world.GetNewEntity();

            var result = entity.Has<TestComponent>();

            world.Dispose();

            Assert.IsFalse(result);
        }

        [Test]
        public void Static()
        {
            var world = Worlds.Create();

            var firstEntity = world.GetNewEntity();
            var secondEntity = world.GetNewEntity();

            firstEntity.ReplaceStatic(new TestStaticValueComponent());

            var firstValue = firstEntity.GetStatic<TestStaticValueComponent>().Value;
            var secondValue = secondEntity.GetStatic<TestStaticValueComponent>().Value;

            var firstResult = firstValue == secondValue;

            ref var testStaticValueComponent = ref firstEntity.GetStatic<TestStaticValueComponent>();

            testStaticValueComponent.Value = 5;

            firstValue = firstEntity.GetStatic<TestStaticValueComponent>().Value;
            secondValue = secondEntity.GetStatic<TestStaticValueComponent>().Value;

            var secondResult = firstValue == secondValue;

            firstEntity.ReplaceStatic(new TestStaticValueComponent { Value = 10 });

            firstValue = firstEntity.GetStatic<TestStaticValueComponent>().Value;
            secondValue = secondEntity.GetStatic<TestStaticValueComponent>().Value;
            
            var thirdResult = firstValue == secondValue;

            world.Dispose();

            Assert.IsTrue(firstResult);
            Assert.IsTrue(secondResult);
            Assert.IsTrue(thirdResult);
        }
    }
}