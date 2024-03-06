using DesertImage.Collections;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

namespace DesertImage.ECS.Tests
{
    public struct TestValueComponent
    {
        public float Value;
    }

    [BurstCompile]
    public unsafe struct TestPerformanceSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        [BurstCompile]
        private struct PerformanceJob : IJob
        {
            public UnsafeArray<TestValueComponent> Values;

            public void Execute()
            {
                Debug.Log("EXECUTE job");
                
                for (var i = 0; i < Values.Length; i++)
                {
                    Values.Get(i).Value++;
                }
            }
        }

        public void Initialize(in World world)
        {
            _group = Filter.Create(world).With<TestValueComponent>().Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var Values = _group.GetComponents<TestValueComponent>();

            var job = new PerformanceJob { Values = Values };
            context.Handle = job.Schedule(context.Handle);

            // for (var i = 0; i < Values.Length; i++)
            // {
            // Values.Get(i).Value++;
            // }
        }
    }
    
    public unsafe struct TestPerformanceSecondSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world).With<TestValueComponent>().Find();
        }

        public void Execute(ref SystemsContext context)
        {
            // for (var i = 0; i < Values.Length; i++)
            // {
            // Values.Get(i).Value++;
            // }
        }
    }

    public class TestStarter : EcsStarter
    {
        protected override void InitSystems()
        {
            for (var i = 0; i < 100_000; i++)
            {
                var entity = World.GetNewEntity();
                entity.Replace<TestValueComponent>();
            }

            World.Add<TestPerformanceSystem>();
        }
    }
}