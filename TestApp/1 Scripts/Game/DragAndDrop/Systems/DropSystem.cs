using DesertImage.ECS;
using Unity.Burst;
using Unity.Jobs;

namespace Game.DragAndDrop
{
    [BurstCompile]
    public struct DropSystem : IInitSystem, IExecuteSystem
    {
        [BurstCompile]
        private struct DropSystemJob : IJob
        {
            public EntitiesGroup Group;

            public void Execute()
            {
                foreach (var i in Group)
                {
                    Group.GetEntity(i).Remove<Drag>();
                }
            }
        }

        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Drag>()
                .With<DropTag>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var job = new DropSystemJob { Group = _group };
            context.Handle = job.Schedule(context.Handle);
            
            // foreach (var i in _group)
            // {
                // _group.GetEntity(i).Remove<Drag>();
            // }
        }
    }
}