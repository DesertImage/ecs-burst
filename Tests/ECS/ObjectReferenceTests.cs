using NUnit.Framework;
using UnityEngine;

namespace DesertImage.ECS
{
    public class ObjectReferenceTests
    {
        [Test]
        public void AssignAndNull()
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

        [Test]
        public void UpdateFromSystem()
        {
            var world = Worlds.Create();

            world.Add<TestObjectReferenceSystem>(ExecutionOrder.EarlyMainThread);

            var obj = new GameObject();
            var rigidbody = obj.AddComponent<Rigidbody>();

            var entity = world.GetNewEntity();
            entity.Replace(new TestReferenceComponent { Rigidbody = rigidbody });

            var firstResult = entity.Get<TestReferenceComponent>().Rigidbody.Value.mass;

            world.Tick(.1f);

            var secondResult = entity.Get<TestReferenceComponent>().Rigidbody.Value.mass;

            Object.DestroyImmediate(obj);
            world.Dispose();

            Assert.AreEqual(1, firstResult);
            Assert.AreEqual(1234, secondResult);
        }
    }
}