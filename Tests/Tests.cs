using System;
using DesertImage.Collections;
using DesertImage.ECS;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public class EcsBaseTests
    {
        [Test]
        public void CheckComponentAdd()
        {
            var world = Worlds.Initialize();

            var entity = world.GetNewEntity();

            entity.Replace(new TestComponent());

            Assert.IsTrue(entity.Has<TestComponent>());

            entity.Remove<TestComponent>();

            Assert.IsFalse(entity.IsAlive());

            world.Dispose();
        }

        [Test]
        public void CheckSharedComponent()
        {
            var world = Worlds.Initialize();

            var firstEntity = world.GetNewEntity();
            var secondEntity = world.GetNewEntity();

            firstEntity.ReplaceShared(new TestSharedValueComponent { Value = 1 });

            Assert.IsTrue(firstEntity.HasShared<TestSharedValueComponent>());
            Assert.IsFalse(secondEntity.HasShared<TestSharedValueComponent>());

            secondEntity.ReplaceShared(firstEntity.GetShared<TestSharedValueComponent>());

            Assert.IsTrue(firstEntity.HasShared<TestSharedValueComponent>());
            Assert.IsTrue(secondEntity.HasShared<TestSharedValueComponent>());

            ref var component = ref secondEntity.GetShared<TestSharedValueComponent>();
            component.Value = 2;

            Assert.AreEqual(2, firstEntity.GetShared<TestSharedValueComponent>().Value);
            Assert.AreEqual(2, secondEntity.GetShared<TestSharedValueComponent>().Value);

            firstEntity.RemoveShared<TestSharedValueComponent>();

            // Assert.IsFalse(firstEntity.HasShared<TestSharedValueComponent>());
            Assert.IsTrue(secondEntity.HasShared<TestSharedValueComponent>());

            world.Dispose();
        }

        [Test]
        public void CheckStaticComponent()
        {
            var world = Worlds.Initialize();

            var firstEntity = world.GetNewEntity();
            var secondEntity = world.GetNewEntity();

            firstEntity.ReplaceStatic(new TestStaticValueComponent { Value = 1 });

            Assert.IsTrue(firstEntity.HasStatic<TestStaticValueComponent>());
            Assert.IsTrue(secondEntity.HasStatic<TestStaticValueComponent>());

            secondEntity.ReplaceStatic(new TestStaticValueComponent { Value = 2 });

            Assert.AreEqual(2, firstEntity.GetStatic<TestStaticValueComponent>().Value);
            Assert.AreEqual(2, secondEntity.GetStatic<TestStaticValueComponent>().Value);

            Assert.IsTrue(firstEntity.HasStatic<TestStaticValueComponent>());
            Assert.IsTrue(secondEntity.HasStatic<TestStaticValueComponent>());

            ref var component = ref secondEntity.GetStatic<TestStaticValueComponent>();
            component.Value = 2;

            var thirdEntity = world.GetNewEntity();

            Assert.IsTrue(thirdEntity.HasStatic<TestStaticValueComponent>());

            Assert.AreEqual(2, firstEntity.GetStatic<TestStaticValueComponent>().Value);
            Assert.AreEqual(2, secondEntity.GetStatic<TestStaticValueComponent>().Value);
            Assert.AreEqual(2, thirdEntity.GetStatic<TestStaticValueComponent>().Value);

            world.Dispose();
        }

        [Test]
        public void CheckHasNull()
        {
            var world = Worlds.Initialize();

            var entity = world.GetNewEntity();

            Assert.IsFalse(entity.Has<TestComponent>());

            world.Dispose();
        }

#if DEBUG
        [Test]
        public void CheckHasOnDeadEntity()
        {
            var world = Worlds.Initialize();

            var entity = new Entity(1);

            try
            {
                entity.Has<TestComponent>();
            }
            catch (Exception)
            {
                Assert.IsTrue(true);
                return;
            }

            Assert.IsTrue(false);

            world.Dispose();
        }
#endif

        [Test]
        public void CheckComponentRemove()
        {
            var world = Worlds.Initialize();
            var entity = world.GetNewEntity();

            entity.Replace(new TestComponent());
            entity.Remove<TestComponent>();

            Assert.IsFalse(entity.IsAlive());

            world.Dispose();
        }

        [Test]
        public void CheckComponentReplace()
        {
            var world = Worlds.Initialize();

            var entity = world.GetNewEntity();

            entity.Replace(new TestValueComponent { Value = 1 });
            entity.Replace(new TestValueComponent { Value = 2 });

            Assert.AreEqual(2, entity.Get<TestValueComponent>().Value);

            world.Dispose();
        }

        [Test]
        public void CheckChangingValueByRef()
        {
            var world = Worlds.Initialize();
            var entity = world.GetNewEntity();

            entity.Replace(new TestValueComponent { Value = 1 });
            entity.Get<TestValueComponent>().Value = 3;

            Assert.AreEqual(3, entity.Get<TestValueComponent>().Value);

            ref var testValueComponent = ref entity.Get<TestValueComponent>();
            testValueComponent.Value = 5;

            Assert.AreEqual(5, entity.Get<TestValueComponent>().Value);

            world.Dispose();
        }

        [Test]
        public void CheckGroupAddRemove()
        {
            var world = Worlds.Initialize();

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

            world.Dispose();
        }

        [Test]
        public void CheckExecuteSystem()
        {
            var world = Worlds.Initialize();

            world.Add<TestValueSystem>();
            world.Add<TestValueSecondSystem>();

            var entity = world.GetNewEntity();
            entity.Replace(new TestValueComponent { Value = 5 });

            world.Tick(.1f);

            Assert.AreEqual(6, entity.Get<TestValueComponent>().Value);

            entity.Replace(new TestComponent());
            world.Tick(.1f);

            Assert.AreEqual(2, entity.Get<TestValueComponent>().Value);

            entity.Remove<TestComponent>();

            world.Tick(.1f);

            Assert.AreEqual(3, entity.Get<TestValueComponent>().Value);

            world.Dispose();
        }

        [Test]
        public void CheckRemoveComponentSystem()
        {
            var world = Worlds.Initialize();

            world.Add<RemoveComponentSystem<TestValueComponent>>();

            var entity = world.GetNewEntity();

            entity.Replace(new TestComponent());

            world.Tick(.1f);

            Assert.IsTrue(entity.Has<TestComponent>());

            entity.Replace(new TestValueComponent { Value = 1 });

            world.Tick(.1f);

            Assert.IsTrue(entity.Has<TestComponent>());
            Assert.IsFalse(entity.Has<TestValueComponent>());

            world.Dispose();

            // var foo = new UnsafeArray<NativeParallelHashMap<int, int>>();
        }

        [Test]
        public unsafe void CheckUnsafeArray()
        {
            var data = new ComponentStorage(20 * 20, 20);

            data.Write(1, 1, new TestValueComponent { Value = 4 });

            var testValueComponent = data.Read<TestValueComponent>(1, 1);

            Assert.AreEqual(4, testValueComponent.Value);

            data.Dispose();
        }
    }
}