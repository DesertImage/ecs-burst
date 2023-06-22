using DesertImage.ECS;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ECS
{
    [BurstCompile]
    public struct TestJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Entity> Input;

        public void Execute(int index)
        {
            var entity = Input[0];

            // ref var testJobComponent = ref entity.Get<TestJobComponent>();
            // testJobComponent.Value = index;
        }
    }

    public struct TestJobComponent
    {
        public int Value;
    }

    public class TestJobs : MonoBehaviour
    {
        private void Start()
        {
            var world = Worlds.Create();

            var newEntity = world.GetNewEntity();

            newEntity.Replace(new TestJobComponent { Value = 1 });

            var input = new NativeArray<Entity>(2, Allocator.TempJob);
            input[0] = newEntity;

            var job = new TestJob
            {
                Input = input,
            };

            var jobHandle = job.Schedule(1_000_000, 100);
            jobHandle.Complete();

#if DEBUG
            UnityEngine.Debug.Log($"[TestJobs] restul: {newEntity.Get<TestJobComponent>().Value}");
#endif

            input.Dispose();
        }
    }
}