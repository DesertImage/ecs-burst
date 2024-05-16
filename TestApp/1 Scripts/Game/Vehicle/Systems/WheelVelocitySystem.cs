using DesertImage.ECS;
using Game.Physics;
using Unity.Mathematics;

namespace Game.Vehicle
{
    public struct WheelVelocitySystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<WheelVelocity>()
                .With<WheelContact>()
                .With<PhysicalObject>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var wheelVelocities = _group.GetComponents<WheelVelocity>();
            var wheelContacts = _group.GetComponents<WheelContact>();
            var physicalObjects = _group.GetComponents<PhysicalObject>();
            var positions = _group.GetComponents<Position>();
            var rotations = _group.GetComponents<Rotation>();

            foreach (var entityId in _group)
            {
                ref var velocity = ref wheelVelocities.Get(entityId);
                var hit = wheelContacts.Read(entityId).Value;
                var rigidbody = physicalObjects.Read(entityId).Rigidbody.Value;

                var position = positions.Read(entityId).Value;
                var rotation = rotations.Get(entityId).Value;

                var worldVelocity = rigidbody.GetPointVelocity(position);

                var right = math.mul(rotation, math.right());
                var forward = math.normalize(math.cross(right, hit.normal));

                velocity.Value = worldVelocity;
                velocity.Forward = math.dot(forward, worldVelocity);
                velocity.Side = math.dot(right, worldVelocity);
            }
        }
    }
}