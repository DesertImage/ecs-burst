using NUnit.Framework;

namespace DesertImage.ECS.Tests
{
    public class GroupsTests
    {
        [Test]
        public void Add()
        {
            var world = Worlds.Create();

            var group = Filter.Create(world).With<TestComponent>().Build();

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

            var group = Filter.Create(world).With<TestComponent>().Build();

            entity.Replace<TestComponent>();
            entity.Remove<TestComponent>();

            var result = group.Contains(entity.Id);

            world.Dispose();

            Assert.IsFalse(result);
        }

        [Test]
        public void FullAddRemove()
        {
            var world = Worlds.Create();

            var entity = world.GetNewEntity();
            var entityId = entity.Id;

            var group = Filter.Create(world).With<TestComponent>().None<TestValueComponent>().Build();

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
        public unsafe void UniversalGroup()
        {
            var world = Worlds.Create();

            var entity = world.GetNewEntity();
            var secondEntity = world.GetNewEntity();
            entity.Replace<TestComponent>();
            secondEntity.Replace<TestComponent>();

            secondEntity.Replace<TestValueComponent>();

            var entityId = entity.Id;

            var group = Filter.Create(world).With<TestComponent>().None<TestValueComponent>().Build();

            var firstResult = group.Contains(entityId);
            var secondResult = group.Contains(secondEntity.Id);

            group.Remove(entityId);

            var thirdResult = group.Contains(entityId);

            world.Dispose();

            Assert.IsTrue(firstResult);
            Assert.IsFalse(secondResult);
            Assert.IsFalse(thirdResult);
        }
    }
}