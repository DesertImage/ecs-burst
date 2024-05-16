using DesertImage.ECS;
using Unity.Jobs;
using Unity.Mathematics;

namespace Game.Boids
{
    public struct NeighboursDetectionSystem : IInitSystem, IExecuteSystem
    {
        private const float Radius = 2f;

        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Neighbours>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var job = new NeighboursDetectionSystemJob { Group = _group };
            context.Handle = job.Schedule(context.Handle);
        }
        
        private struct NeighboursDetectionSystemJob : IJob
        {
            public EntitiesGroup Group;

            public unsafe void Execute()
            {
                var neighboursList = Group.GetComponents<Neighbours>();
                var positions = Group.GetComponents<Position>();

                foreach (var i in Group)
                {
                    ref var neighbours = ref neighboursList.Get(i);
                    var originPosition = positions[i].Value;

                    foreach (var j in Group)
                    {
                        if (i == j) continue;
                        if (neighbours.Values.Contains(j)) continue;

                        var comparePosition = positions[j].Value;

                        var distance = math.lengthsq(comparePosition - originPosition);

                        if (distance > Radius) continue;

                        neighbours.Values.Set(j, j);
                    }

                    var values = neighbours.Values;
                    for (var j = values.Count - 1; j >= 0; j--)
                    {
                        var neighbourId = values.Values[j];
                        var comparePosition = positions[neighbourId].Value;

                        var distance = math.lengthsq(comparePosition - originPosition);

                        if (distance < Radius) continue;

                        neighbours.Values.Remove(neighbourId);
                    }
                }
            }
        }
    }
}