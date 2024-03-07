using System;
using DesertImage.Collections;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

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
    }
}