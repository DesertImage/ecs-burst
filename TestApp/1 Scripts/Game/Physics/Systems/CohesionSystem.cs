using DesertImage.ECS;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Physics
{
    public struct VelocitySystem : IInitSystem, IExecuteSystem
    {
        private struct VelocitySystemJob : IJob
        {
            public EntitiesGroup Group;
            public float DeltaTime;

            public void Execute()
            {
                var velocities = Group.GetComponents<Velocity>();
                var positions = Group.GetComponents<Position>();

                foreach (var i in Group)
                {
                    var velocity = velocities[i].Value;
                    positions.Get(i).Value += velocity * DeltaTime;
                }
            }
        }

        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Velocity>()
                .With<Position>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var job = new VelocitySystemJob
            {
                Group = _group,
                DeltaTime = context.DeltaTime
            };

            context.Handle = job.Schedule(context.Handle);
        }
    }
}