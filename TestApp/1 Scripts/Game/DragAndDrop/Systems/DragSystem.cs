using DesertImage.ECS;
using Unity.Burst;
using Unity.Jobs;

namespace Game.DragAndDrop
{
    [BurstCompile]
    public struct DragSystem : IInitSystem, IExecuteSystem
    {
        [BurstCompile]
        private struct DragSystemJob : IJob
        {
            public EntitiesGroup Group;

            public void Execute()
            {
                var drags = Group.GetComponents<Drag>();
                var positions = Group.GetComponents<Position>();
                foreach (var i in Group)
                {
                    positions.Get(i).Value = drags[i].Position;
                }
            }
        }

        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Drag>()
                .With<Position>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var job = new DragSystemJob { Group = _group };
            context.Handle = job.Schedule(context.Handle);
        }
    }
}