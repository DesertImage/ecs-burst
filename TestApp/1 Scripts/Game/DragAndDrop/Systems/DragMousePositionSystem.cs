using DesertImage.Collections;
using DesertImage.ECS;
using Game.Input;
using Unity.Burst;
using Unity.Jobs;

namespace Game.DragAndDrop
{
    [BurstCompile]
    public struct DragMousePositionSystem : IInitSystem, IExecuteSystem
    {
        [BurstCompile]
        private struct DragMousePositionJob : IJob
        {
            public EntitiesGroup Group;
            public UnsafeReadOnlyArray<Drag> Data;
            public MouseWorldPosition MouseWorldPosition;

            public void Execute()
            {
                var drags = Group.GetComponents<Drag>();
                foreach (var i in Group)
                {
                    drags.Get(i).Position = MouseWorldPosition.Value;
                }
            }
        }

        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Drag>()
                .Find();
        }

        public unsafe void Execute(ref SystemsContext context)
        {
            var job = new DragMousePositionJob
            {
                Group = _group,
                MouseWorldPosition = context.World.ReadStatic<MouseWorldPosition>()
            };

            context.Handle = job.Schedule(context.Handle);
        }
    }
}