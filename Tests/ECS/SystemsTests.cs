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

            world.Add<TestValueSystem>(ExecutionOrder.EarlyMainThread);

            world.Tick(.1f);

            var firstResult = entity.Read<TestValueComponent>().Value;

            entity.Replace<TestComponent>();

            world.Tick(.1f);

            var secondResult = entity.Read<TestValueComponent>().Value;

            entity.Remove<TestComponent>();

            world.Tick(.1f);

            var thirdResult = entity.Read<TestValueComponent>().Value;

            entity.Remove<TestValueComponent>
            (
#if ECS_AUTODESTROY_ENTITY
                true
#endif
            );

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

            const ExecutionOrder executionType = ExecutionOrder.MultiThread;
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

            world.Add<TestValueSystem>(ExecutionOrder.EarlyMainThread);

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
    }
}