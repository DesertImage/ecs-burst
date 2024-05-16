using DesertImage.ECS;
using Game.Physics;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Vehicle
{
    public struct WheelSimpleFrictionSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Wheel>()
                .With<WheelVelocity>()
                .With<WheelFriction>()
                .With<Suspension>()
                .With<PhysicalObject>()
                .With<Rotation>()
                .With<WheelContact>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var deltaTime = context.DeltaTime;

            var wheels = _group.GetComponents<Wheel>();
            var wheelVelocitites = _group.GetComponents<WheelVelocity>();
            var wheelFrictions = _group.GetComponents<WheelFriction>();
            var suspensions = _group.GetComponents<Suspension>();
            var rotations = _group.GetComponents<Rotation>();
            var physicalObjects = _group.GetComponents<PhysicalObject>();
            var groundContacts = _group.GetComponents<WheelContact>();

            foreach (var entityId in _group)
            {
                ref var friction = ref wheelFrictions.Get(entityId);

                var wheel = wheels.Read(entityId);
                var velocity = wheelVelocitites.Read(entityId);
                var suspensionForce = suspensions.Read(entityId).Force;
                var rigidbody = physicalObjects.Read(entityId).Rigidbody.Value;
                var hit = groundContacts.Read(entityId).Value;
                var rotation = rotations.Read(entityId).Value;

                if (suspensionForce <= 0f) continue;

                var wheelRadius = wheel.Radius;

                //lateral
                var lateralDirection = math.mul(rotation, math.right());
                var lateralVelocity = velocity.Side;
                var lateralVelocitySign = math.sign(lateralVelocity);
                var lateralVelocityAbs = lateralVelocitySign * lateralVelocity;
                var lateralForce = -lateralVelocitySign * suspensionForce * lateralVelocityAbs * lateralDirection;

                //longitudinal
                var longDirection = math.normalize(math.cross(lateralDirection, hit.normal));
                var wheelDelta = wheel.AngularVelocity.AngularToLinear(wheelRadius) - velocity.Forward;
                var longVelocitySign = math.sign(wheelDelta);
                var longVelocityAbs = longVelocitySign * wheelDelta;

                var longForce = suspensionForce * wheelDelta * longDirection;

                if (longVelocityAbs > 1f)
                {
                    wheel.AngularVelocity -= longVelocitySign * longVelocityAbs.LinearToAngular(wheelRadius);
                }
                else
                {
                    wheel.AngularVelocity -= wheelDelta.LinearToAngular(wheelRadius);
                }
                
                rigidbody.AddForceAtPosition
                (
                    (longForce + lateralForce) * deltaTime,
                    hit.point,
                    ForceMode.Impulse
                );
            }
        }
    }
}