using DesertImage.ECS;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Boids
{
    public struct CohesionSystem : IInitSystem, IExecuteSystem
    {
        private struct CohesionSystemJob : IJob
        {
            public EntitiesGroup Group;

            public unsafe void Execute()
            {
                var neighboursList = Group.GetComponents<Neighbours>();
                var positions = Group.GetComponents<Position>();
                var cohesions = Group.GetComponents<Cohesion>();

                foreach (var i in Group)
                {
                    ref var cohesion = ref cohesions.Get(i);
                    var neighbours = neighboursList[i].Values;

                    var originPosition = positions[i].Value;

                    var newCohesion = float3.zero;
                    newCohesion += originPosition;

                    for (var j = 0; j < neighbours.Count; j++)
                    {
                        var neighbourId = neighbours.Values[j];
                        var neighbourPosition = positions[neighbourId].Value;
                        newCohesion += neighbourPosition;
                    }

                    newCohesion /= neighbours.Count + 1;
                    newCohesion -= originPosition;

                    cohesion.Value = newCohesion;
                    
                    Debug.DrawLine(originPosition, newCohesion, Color.red);
                }
            }
        }

        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Neighbours>()
                .With<Cohesion>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var job = new CohesionSystemJob { Group = _group };
            context.Handle = job.Schedule(context.Handle);
        }
    }
}