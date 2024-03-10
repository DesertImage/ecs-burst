using DesertImage.Collections;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

namespace DesertImage.ECS
{
    [BurstCompile]
    public struct TestValueJobSystem : IInitSystem, ICalculateSystem
    {
        private EntitiesGroup _group;

        [BurstCompile]
        private struct TestJob : IJob
        {
            public UnsafeReadOnlyArray<TestValueComponent> Values;

            public void Execute()
            {
                for (var i = 0; i < Values.Length; i++)
                {
                    Values.Get(i).Value++;
                }
            }
        }

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<TestValueComponent>()
                .None<TestComponent>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var unsafeUintReadOnlyArray = _group.GetComponents<TestValueComponent>();
            var job = new TestJob { Values = unsafeUintReadOnlyArray.Values };
            context.Handle = job.Schedule(context.Handle);
        }
    }
}