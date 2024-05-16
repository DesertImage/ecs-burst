using DesertImage.ECS;
using Game.Physics;
using Unity.Mathematics;

namespace Game.Vehicle
{
    public struct WheelContactSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Suspension>()
                .With<PhysicalObject>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var suspensions = _group.GetComponents<Suspension>();
            var positions = _group.GetComponents<Position>();
            var rotations = _group.GetComponents<Rotation>();

            foreach (var entityId in _group)
            {
                ref var suspension = ref suspensions.Get(entityId);
                var position = positions.Read(entityId).Value;
                var rotation = rotations.Get(entityId).Value;
                var up = math.mul(rotation, math.up());

                var entity = _group.GetEntity(entityId);

                if (!UnityEngine.Physics.Raycast(position, -up, out var hit, suspension.Height))
                {
                    if (entity.Has<WheelContact>())
                    {
                        entity.Remove<WheelContact>();
                    }

                    continue;
                }

                entity.Replace(new WheelContact { Value = hit });
            }
        }
    }
}