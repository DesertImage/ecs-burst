using NUnit.Framework;

namespace DesertImage.ECS
{
    public class EcsBaseTests
    {
        [Test]
        public void CheckComponentAdd()
        {
            var world = new World();
            var entity = world.GetNewEntity();

            entity.Replace(new TestComponent());

            Assert.IsTrue(entity.Has<TestComponent>());

            entity.Remove<TestComponent>();

            Assert.IsFalse(entity.Has<TestComponent>());
        }

        [Test]
        public void CheckComponentRemove()
        {
            var world = new World();
            var entity = world.GetNewEntity();

            entity.Replace(new TestComponent());
            entity.Remove<TestComponent>();

            Assert.IsFalse(entity.Has<TestComponent>());
        }

        [Test]
        public void CheckComponentReplace()
        {
            var world = new World();
            var entity = world.GetNewEntity();

            entity.Replace(new TestValueComponent { Value = 1 });
            entity.Replace(new TestValueComponent { Value = 2 });

            Assert.AreEqual(2, entity.Get<TestValueComponent>().Value);
        }

        [Test]
        public void CheckChangingValueByRef()
        {
            var world = new World();
            var entity = world.GetNewEntity();

            entity.Replace(new TestValueComponent { Value = 1 });
            entity.Get<TestValueComponent>().Value = 2;

            Assert.AreEqual(2, entity.Get<TestValueComponent>().Value);
        }

        [Test]
        public void CheckGroupAddRemove()
        {
            var world = new World();

            var entity = world.GetNewEntity();
            var entityId = entity.Id;

            var group = world.GetGroup(MatcherBuilder.Create().With<TestComponent>().Build());
            var group2 = world.GetGroup
            (
                MatcherBuilder.Create().With<TestComponent>().None<TestValueComponent>().Build()
            );

            entity.Replace(new TestComponent());

            Assert.IsTrue(group.Entities.Contains(entityId));
            Assert.IsTrue(group2.Entities.Contains(entityId));

            entity.Replace(new TestValueComponent());

            Assert.IsTrue(group.Entities.Contains(entityId));
            Assert.IsFalse(group2.Entities.Contains(entityId));

            entity.Remove<TestValueComponent>();

            Assert.IsTrue(group.Entities.Contains(entityId));
            Assert.IsTrue(group2.Entities.Contains(entityId));

            entity.Remove<TestComponent>();

            Assert.IsFalse(group.Entities.Contains(entityId));
            Assert.IsFalse(group2.Entities.Contains(entityId));
        }

        [Test]
        public void CheckExecuteSystem()
        {
            var world = new World();
            world.Add<TestValueSystem>();

            var entity = world.GetNewEntity();
            entity.Replace(new TestValueComponent { Value = 1 });

            world.Tick(.1f);

            Assert.AreEqual(2, entity.Get<TestValueComponent>().Value);

            entity.Replace(new TestComponent());
            world.Tick(.1f);

            Assert.AreEqual(2, entity.Get<TestValueComponent>().Value);

            entity.Remove<TestComponent>();

            world.Tick(.1f);

            Assert.AreEqual(3, entity.Get<TestValueComponent>().Value);
        }

        [Test]
        public void CheckRemoveComponentSystem()
        {
            var world = new World();
            world.Add<RemoveComponentSystem<TestValueComponent>>();

            var entity = world.GetNewEntity();

            entity.Replace(new TestComponent());

            world.Tick(.1f);

            Assert.IsTrue(entity.Has<TestComponent>());

            entity.Replace(new TestValueComponent { Value = 1 });

            world.Tick(.1f);
            
            Assert.IsTrue(entity.Has<TestComponent>());
            Assert.IsFalse(entity.Has<TestValueComponent>());
        }
    }
}