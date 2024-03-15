using DesertImage.ECS;
using Unity.Burst;
using Unity.Jobs;

namespace Game.DragAndDrop
{
    [BurstCompile]
    public struct DragLocalPositionSystem : IInitSystem, IExecuteSystem
    {
        [BurstCompile]
        private struct DragLocalPositionSystemJob : IJob
        {
            public EntitiesGroup Group;

            public void Execute()
            {
                var drags = Group.GetComponents<Drag>();
                var localPositions = Group.GetComponents<LocalPosition>();
                var originPositions = Group.GetComponents<OriginPosition>();
                foreach (var i in Group)
                {
                    var dragPosition = drags[i].Position;
                    var originPosition = originPositions[i];
                    localPositions.Get(i).Value = dragPosition - originPosition.Value;
                }
            }
        }

        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Drag>()
                .With<OriginPosition>()
                .With<LocalPosition>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var job = new DragLocalPositionSystemJob { Group = _group };
            context.Handle = job.Schedule(context.Handle);
        }
    }
}