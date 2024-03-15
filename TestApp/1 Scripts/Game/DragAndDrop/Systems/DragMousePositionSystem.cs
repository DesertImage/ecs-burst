using DesertImage.Collections;
using DesertImage.ECS;
using Game.Input;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

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
            public MousePosition MousePosition;

            public void Execute()
            {
                var drags = Group.GetComponents<Drag>();
                foreach (var i in Group)
                {
                    drags.Get(i).Position = MousePosition.WorldPosition;
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
                MousePosition = context.World.GetStatic<MousePosition>()
            };
            
            context.Handle = job.Schedule(context.Handle);
        }
    }
}