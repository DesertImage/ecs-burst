using DesertImage.Collections;
using NUnit.Framework;
using Unity.Collections;

namespace DesertImage.ECS.Tests
{
    public class GroupsTests
    {
        [Test]
        public void Add()
        {
            var world = Worlds.Create();

            var group = Filter.Create(world).With<TestComponent>().Find();

            var entity = world.GetNewEntity();
            entity.Replace<TestComponent>();

            var result = group.Contains(entity.Id);

            world.Dispose();

            Assert.IsTrue(result);
        }

        [Test]
        public void Remove()
        {
            var world = Worlds.Create();

            var entity = world.GetNewEntity();

            var group = Filter.Create(world).With<TestComponent>().Find();

            entity.Replace<TestComponent>();
            entity.Remove<TestComponent>();

            var result = group.Contains(entity.Id);

            world.Dispose();

            Assert.IsFalse(result);
        }

        [Test]
        public void Count()
        {
            var world = Worlds.Create();

            var group = Filter.Create(world)
                .With<TestComponent>()
                .With<TestValueComponent>()
                .None<TestValueSecondComponent>()
                .Find();

            const int entitiesCount = 2;

            var entities = new UnsafeArray<Entity>(entitiesCount, Allocator.Persistent);
            var results = new UnsafeArray<int>(5, Allocator.Persistent);

            for (var i = 0; i < entitiesCount; i++)
            {
                var entity = world.GetNewEntity();

                entities[i] = entity;

                entity.Replace<TestComponent>();
                entity.Replace<TestValueComponent>();
            }

            results[0] = group.Count;

            for (var i = 0; i < entitiesCount; i++)
            {
                entities[i].Remove<TestValueComponent>();
            }

            results[1] = group.Count;

            for (var i = 0; i < entitiesCount; i++)
            {
                var entity = entities[i];
                entity.Replace<TestValueComponent>();
            }

            results[2] = group.Count;

            for (var i = 0; i < entitiesCount; i++)
            {
                entities[i].Replace<TestValueSecondComponent>();
            }

            results[3] = group.Count;

            for (var i = 0; i < entitiesCount; i++)
            {
                entities[i].Remove<TestValueSecondComponent>();
            }

            results[4] = group.Count;

            world.Dispose();
            entities.Dispose();
            results.Dispose();

            Assert.AreEqual(entitiesCount, results[0]);
            Assert.AreEqual(0, results[1]);
            Assert.AreEqual(entitiesCount, results[2]);
            Assert.AreEqual(0, results[3]);
            Assert.AreEqual(entitiesCount, results[4]);
        }

        [Test]
        public void FullAddRemove()
        {
            var world = Worlds.Create();

            var entity = world.GetNewEntity();
            var entityId = entity.Id;

            var group = Filter.Create(world).With<TestComponent>().None<TestValueComponent>().Find();

            entity.Replace<TestComponent>();

            var firstResult = group.Contains(entityId);

            entity.Replace<TestValueComponent>();

            var secondResult = group.Contains(entityId);

            entity.Remove<TestValueComponent>();

            var thirdResult = group.Contains(entityId);

            entity.Replace<TestReferenceComponent>();

            var fourthResult = group.Contains(entityId);

            world.Dispose();

            Assert.IsTrue(firstResult);
            Assert.IsFalse(secondResult);
            Assert.IsTrue(thirdResult);
            Assert.IsTrue(fourthResult);
        }

        [Test]
        public void NewGroup()
        {
            var world = Worlds.Create();

            var entity = world.GetNewEntity();
            var secondEntity = world.GetNewEntity();
            entity.Replace<TestComponent>();
            secondEntity.Replace<TestComponent>();

            secondEntity.Replace<TestValueComponent>();

            var entityId = entity.Id;

            var group = Filter.Create(world).With<TestComponent>().None<TestValueComponent>().Find();

            var firstResult = group.Contains(entityId);
            var secondResult = group.Contains(secondEntity.Id);

            group.Remove(entityId);

            var thirdResult = group.Contains(entityId);

            world.Dispose();

            Assert.IsTrue(firstResult);
            Assert.IsFalse(secondResult);
            Assert.IsFalse(thirdResult);
        }

        [Test]
        public void GetComponents()
        {
            var world = Worlds.Create();

            const int entitiesCount = 2;
            var entities = new UnsafeArray<Entity>(entitiesCount, Allocator.Persistent);
            var results = new UnsafeArray<int>(5, Allocator.Persistent);

            for (var i = 0; i < entitiesCount; i++)
            {
                var entity = world.GetNewEntity();
                entities[i] = entity;
                entity.Replace<TestValueComponent>();
            }

            var group = Filter.Create(world).With<TestValueComponent>().Find();

            var testValueComponents = group.GetComponents<TestValueComponent>();
            for (var i = testValueComponents.Length - 1; i >= 0; i--)
            {
                testValueComponents.Get(i).Value += 1;
            }

            for (var i = 0; i < entitiesCount; i++)
            {
                Assert.AreEqual
                (
                    1,
                    entities[i].Read<TestValueComponent>().Value
                );
            }

            world.Dispose();
            entities.Dispose();
            results.Dispose();
        }
    }
}