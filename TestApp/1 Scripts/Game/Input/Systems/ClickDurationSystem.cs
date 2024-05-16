using DesertImage.Collections;
using DesertImage.ECS;
using Unity.Burst;
using Unity.Jobs;

namespace Game.Input
{
    [BurstCompile]
    public struct ClickDurationSystem : IInitSystem, IExecuteSystem
    {
        [BurstCompile]
        private struct ClickDurationSystemJob : IJob
        {
            public EntitiesGroup Group;
            public float DeltaTime;

            public void Execute()
            {
                var clicks = Group.GetComponents<Click>();
                foreach (var i in Group)
                {
                    clicks.Get(i).Duration += DeltaTime;
                }
            }
        }

        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Click>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var job = new ClickDurationSystemJob
            {
                Group = _group,
                DeltaTime = context.DeltaTime
            };

            context.Handle = job.Schedule(context.Handle);
        }
    }
}