using DesertImage.Collections;
using Unity.Jobs;

namespace DesertImage.ECS
{
    public struct TestValueJobSystem : IInitSystem, ICalculateSystem
    {
        private EntitiesGroup _group;

        private struct TestJob : IJob
        {
            public UnsafeArray<TestValueComponent> Values;

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
            var job = new TestJob { Values = _group.GetComponents<TestValueComponent>() };
            context.Handle = job.Schedule(context.Handle);
        }
    }
}