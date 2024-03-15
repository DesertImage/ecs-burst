using System;
using DesertImage.Collections;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Random = UnityEngine.Random;

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
        public void ComplexComponentsTest()
        {
            var world = Worlds.Create();

            const int entitiesCount = 2;
            var entities = new Entity[entitiesCount];
            var results = new TestComplexComponent[entitiesCount];

            for (var i = 0; i < entitiesCount; i++)
            {
                var entity = world.GetNewEntity();
                var entityId = entity.Id;

                entity.Replace
                (
                    new TestComplexComponent
                    {
                        Value = (int)entityId,
                        Value2 = (int)entityId,
                        Value3 = (int)entityId,
                        Float = i,
                        Float2 = i,
                        Float3 = i,
                        TargetTime = i + 1,
                    }
                );

                entity.Replace<TestTag>();

                entities[i] = entity;
            }

            world.Add<TestComplexSystem>();

            const float deltaTimeStep = .1f;
            const float times = 1f / deltaTimeStep;

            for (var i = 0; i < times; i++)
            {
                world.Tick(deltaTimeStep);
            }

            for (var i = 1; i < entitiesCount; i++)
            {
                results[i] = entities[i].Read<TestComplexComponent>();
            }

            world.Dispose();

            for (var i = 1; i < entitiesCount; i++)
            {
                var result = results[i];
                var entityId = i + 1;

                Assert.AreEqual(entityId, result.Value);
                Assert.AreEqual(entityId, result.Value2);
                Assert.AreEqual(entityId, result.Value3);

                Assert.AreEqual(i, result.Float);
                Assert.AreEqual(i, result.Float2);
                Assert.AreEqual(i, result.Float3);

                Assert.AreEqual(i + 1, result.TargetTime);
            }
        }

        [Test]
        public void ComponentsChecking()
        {
            const int entitiesCount = 100;

            var entities = new Entity[entitiesCount];
            var results = new int[entitiesCount];

            var world = Worlds.Create();

            for (var i = 0; i < entitiesCount; i++)
            {
                var entity = world.GetNewEntity();
                entities[i] = entity;

                entity.Replace(new TestValueComponent { Value = (int)entity.Id });
            }

            for (var i = 0; i < entitiesCount; i++)
            {
                var range = Random.Range(0, 3);

                if (range == 0) continue;

                var entity = entities[i];

                if (range == 1)
                {
                    entity.Replace(new TestValueComponent { Value = (int)entity.Id });
                }
                else
                {
                    entity.Remove<TestValueComponent>();
                }
            }

            for (var i = 0; i < entitiesCount; i++)
            {
                var entity = entities[i];
                results[i] = entity.Has<TestValueComponent>() ? entity.Read<TestValueComponent>().Value : -1;
            }

            world.Dispose();

            for (var i = 0; i < entitiesCount; i++)
            {
                var result = results[i];

                if (result == -1) continue;

                Assert.AreEqual(i + 1, result);
            }
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

        [Test]
        public unsafe void ComponentStorageManyEntities()
        {
            const int entitiesCount = 100_000;
            const int componentsCapacity = 1;
            const int entitiesCapacity = 512;

            var storage = new ComponentStorage(componentsCapacity, entitiesCapacity);

            var componentId = ComponentTools.GetComponentId<TestValueComponent>();

            for (uint i = 0; i < entitiesCount; i++)
            {
                storage.Set(i, new TestValueComponent { Value = (int)i });
                storage.Get<TestValueComponent>(i, componentId);
            }

            storage.Dispose();
        }

        [Test]
        public unsafe void PtrTest()
        {
            var size = UnsafeUtility.SizeOf<TestValueComponent>();
            var sizeSecond = UnsafeUtility.SizeOf<TestValueSecondComponent>();

            var ptrSize = IntPtr.Size;

            var testBuffer = (void**)UnsafeUtility.Malloc(ptrSize * 4, 0, Allocator.Persistent);

            var sparseSet = new UnsafeUintUnknownTypeSparseSet(2, 2, size);
            var sparseSetSecond = new UnsafeUintUnknownTypeSparseSet(2, 2, sizeSecond);

            sparseSet.Set(0, new TestValueComponent { Value = 3 });
            sparseSet.Set(1, new TestValueComponent { Value = 6 });

            sparseSetSecond.Set(0, new TestValueSecondComponent { Value = 11 });
            sparseSetSecond.Set(1, new TestValueSecondComponent { Value = 44 });

            var array = new UnsafeArray<TestValueComponent>
            (
                (TestValueComponent*)sparseSet.Values,
                sparseSet.Count,
                Allocator.Persistent
            );

            var arraySecond = new UnsafeArray<TestValueSecondComponent>
            (
                (TestValueSecondComponent*)sparseSetSecond.Values,
                sparseSetSecond.Count,
                Allocator.Persistent
            );

            testBuffer[0] = sparseSet.GetPtr(0);
            testBuffer[1] = sparseSet.GetPtr(1);
            testBuffer[2] = sparseSetSecond.GetPtr(0);
            testBuffer[3] = sparseSetSecond.GetPtr(1);

            ref var firstResult = ref *(TestValueComponent*)testBuffer[0];
            ref var secondResult = ref *(TestValueComponent*)testBuffer[1];
            ref var thirdResult = ref *(TestValueSecondComponent*)testBuffer[2];
            ref var fourthResult = ref *(TestValueSecondComponent*)testBuffer[3];

            sparseSet.Get<TestValueComponent>(0).Value = 50;
            sparseSetSecond.Get<TestValueSecondComponent>(0).Value = 12;
            array.Get(0).Value = 60;
            var sparseResult = sparseSet.Read<TestValueComponent>(0);

            UnsafeUtility.Free(testBuffer, Allocator.Persistent);
            sparseSet.Dispose();
            sparseSetSecond.Dispose();
            array.Dispose();
            arraySecond.Dispose();

            // Assert.AreEqual(0, firstFirst);
            // Assert.AreEqual(1, secondFirst);
            // Assert.AreEqual(0, bufferFirst);
            //
            // Assert.AreEqual(1, firstSecond);
            // Assert.AreEqual(2, secondSecond);
            // Assert.AreEqual(1, bufferSecond);

            // Assert.AreEqual(3, firstThird);
            // Assert.AreEqual(3, secondThird);
            // Assert.AreEqual(3, bufferThird);
        }

        private struct TestContainer
        {
            public UnsafeUintHashSet Data;

            public void Add(uint value) => Data.Add(value);
            public void Dispose() => Data.Dispose();
        }

        [Test]
        public void ResizeTest()
        {
            const int count = 3;
            
            var data = new TestContainer { Data = new UnsafeUintHashSet(count, Allocator.Persistent) };

            for (var i = 0; i < count; i++)
            {
                data.Add((uint)(count + i));
            }

            data.Dispose();
        }
    }
}