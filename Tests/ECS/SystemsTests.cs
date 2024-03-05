using System.Diagnostics;
using DesertImage.Collections;
using NUnit.Framework;
using Unity.Collections;
using Debug = UnityEngine.Debug;

namespace DesertImage.ECS.Tests
{
    public class SystemsTests
    {
        [Test]
        public void SystemExecute()
        {
            var world = Worlds.Create();

            var entity = world.GetNewEntity();

            entity.Replace(new TestValueComponent { Value = 2 });

            world.Add<TestValueSystem>(ExecutionType.MainThread);

            world.Tick(.1f);

            var firstResult = entity.Read<TestValueComponent>().Value;

            entity.Replace<TestComponent>();

            world.Tick(.1f);

            var secondResult = entity.Read<TestValueComponent>().Value;

            entity.Remove<TestComponent>();

            world.Tick(.1f);

            var thirdResult = entity.Read<TestValueComponent>().Value;

            entity.Remove<TestValueComponent>();

            world.Tick(.1f);

            entity.Replace(new TestValueComponent { Value = 1 });

            world.Tick(.1f);

            var fourthResult = entity.Read<TestValueComponent>().Value;

            world.Dispose();

            Assert.AreEqual(3, firstResult);
            Assert.AreEqual(3, secondResult);
            Assert.AreEqual(4, thirdResult);
            Assert.AreEqual(2, fourthResult);
        }

        [Test]
        public void SystemExecuteRemove()
        {
            var world = Worlds.Create();

            var entity = world.GetNewEntity();

            entity.Replace(new TestValueComponent { Value = 1 });

            const ExecutionType executionType = ExecutionType.MultiThread;
            world.Add<TestValueRemoveSystem>(executionType);
            world.Add<TestValueSystem>(executionType);

            world.Tick(.1f);
            world.Tick(.1f);

            world.Dispose();
        }

        [Test]
        public void ExecutionTypeBenchmark()
        {
            const int entitiesCount = 50_000;

            var stopwatch = new Stopwatch();

            var world = Worlds.Create();

            for (var i = 0; i < entitiesCount; i++)
            {
                var entity = world.GetNewEntity();
                entity.Replace<TestValueComponent>();
            }

            world.Add<TestValueSystem>(ExecutionType.MainThread);

            stopwatch.Start();

            world.Tick(.1f);

            stopwatch.Stop();

            var singleThreadResult = stopwatch.Elapsed.TotalMilliseconds;

            world.Remove<TestValueSystem>();
            world.Add<TestValueJobSystem>();

            stopwatch.Restart();

            world.Tick(.1f);

            stopwatch.Stop();

            world.Dispose();

            var multiThreadResult = stopwatch.Elapsed.TotalMilliseconds;

            Debug.Log($"Single: {singleThreadResult}");
            Debug.Log($"Multi: {multiThreadResult}");
        }

        [Test]
        public void RaceCondition()
        {
            const int entitiesCount = 5;

            var data = new UnsafeArray<Entity>(entitiesCount, true, Allocator.Persistent);
            var results = new UnsafeArray<int>(entitiesCount, true, Allocator.Persistent);
            var secondResults = new UnsafeArray<bool>(entitiesCount, true, Allocator.Persistent);

            var world = Worlds.Create();

            for (var i = 0; i < entitiesCount; i++)
            {
                var entity = world.GetNewEntity();
                data[i] = entity;
                entity.Replace<TestValueComponent>();
            }

            const ExecutionType executionType = ExecutionType.MultiThread;
            world.Add<TestValueSystem>(executionType);
            world.Add<TestValueSecondSystem>(executionType);

            world.Tick(.1f);

            for (var i = 0; i < data.Length; i++)
            {
                var entity = data[i];
                var component = entity.Read<TestValueComponent>();
                results[i] = component.Value;
            }

            world.Add<TestValueThirdSystem>(executionType);

            world.Tick(.1f);
            world.Tick(.1f);

            for (var i = 0; i < data.Length; i++)
            {
                var entity = data[i];
                secondResults[i] = entity.Has<TestValueComponent>();
            }

            world.Dispose();
            data.Dispose();
            results.Dispose();
            secondResults.Dispose();

            for (var i = 0; i < results.Length; i++)
            {
                Assert.AreEqual(2, results[i]);
            }

            for (var i = 0; i < secondResults.Length; i++)
            {
                Assert.IsFalse(secondResults[i]);
            }
        }
    }
}