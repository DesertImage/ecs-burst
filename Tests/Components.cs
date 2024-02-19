using NUnit.Framework;
using UnityEngine;

namespace DesertImage.ECS.Tests
{
    public class Components
    {
        [Test]
        public void CheckAddRemove()
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
        public void CheckChangeValue()
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
        public void CheckComponentsAfterPool()
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
        public void CheckStatic()
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

            world.Dispose();

            Assert.IsTrue(firstResult);
            Assert.IsTrue(secondResult);
        }

        [Test]
        public void CheckObjectReference()
        {
            var world = Worlds.Create();

            var entity = world.GetNewEntity();

            var gameObject = new GameObject("test");
            var rigidbody = gameObject.AddComponent<Rigidbody>();

            entity.Replace
            (
                new TestReferenceComponent { Rigidbody = rigidbody }
            );

            bool firstResult = entity.Get<TestReferenceComponent>().Rigidbody.Value;

            ref var component = ref entity.Get<TestReferenceComponent>();
            component.Rigidbody = default;

            bool secondResult = entity.Get<TestReferenceComponent>().Rigidbody.Value;

            component.Rigidbody = rigidbody;
            
            bool thirdResult = entity.Get<TestReferenceComponent>().Rigidbody.Value;

            Object.DestroyImmediate(gameObject);
            world.Dispose();

            Assert.IsTrue(firstResult);
            Assert.IsFalse(secondResult);
            Assert.IsTrue(thirdResult);
        }
    }
}