using DesertImage.ECS;
using Game.Physics;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Vehicle
{
    public struct WheelFrictionMySystem : IInitSystem, IExecuteSystem
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
                ref var wheel = ref wheels.Get(entityId);

                var velocity = wheelVelocitites.Read(entityId);
                var suspensionForce = suspensions.Read(entityId).Force;
                var rigidbody = physicalObjects.Read(entityId).Rigidbody.Value;
                var hit = groundContacts.Read(entityId).Value;
                var rotation = rotations.Read(entityId).Value;

                if (suspensionForce <= 0f) continue;

                var mass = rigidbody.mass * .25f;

                //side
                var sideDirection = math.mul(rotation, math.right());
                var sideVelocity = velocity.Side;
                var sideVelocitySign = math.sign(sideVelocity);
                var sideVelocityAbs = sideVelocity * sideVelocitySign;
                var sideFriction = friction.FrictionCurve.Evaluate(math.saturate(sideVelocityAbs));
                // var sideFriction = 1f;

                var acceleration = -sideVelocity * sideFriction / deltaTime;
                var sideForce = acceleration * mass * sideDirection;

                //forward
                var wheelDeltaVelocity = velocity.Forward - wheel.AngularVelocity.AngularToLinear(wheel.Radius);
                var wheelDeltaVelocitySign = math.sign(wheelDeltaVelocity);
                var wheelDeltaVelocityAbs = wheelDeltaVelocitySign * wheelDeltaVelocity;

                var forwardFriction = friction.FrictionCurve.Evaluate(wheelDeltaVelocity);

                var forwardFrictionForce = -wheelDeltaVelocitySign * forwardFriction *
                                           suspensionForce *
                                           Bias(math.saturate(wheelDeltaVelocityAbs), -1);

                var forwardDirection = math.normalize(math.cross(sideDirection, hit.normal));
                var forwardForce = forwardFrictionForce * forwardDirection;

                rigidbody.AddForceAtPosition
                (
                    (forwardForce + sideForce) * deltaTime,
                    hit.point,
                    ForceMode.Impulse
                );

                var toNeutralForce = (-wheelDeltaVelocity.LinearToAngular(wheel.Radius) * wheel.Inertia / deltaTime)
                    .TorqueToForce(wheel.Radius);

                var usedForceValue = math.abs(toNeutralForce) > math.abs(forwardFrictionForce)
                    ? forwardFrictionForce
                    : toNeutralForce;

                wheel.AngularVelocity -= usedForceValue.ForceToTorque(wheel.Radius) / wheel.Inertia * deltaTime;
            }
        }

        private static float Bias(float x, float bias)
        {
            var k = Mathf.Pow(1 - bias, 3);
            return (x * k) / (x * k - x + 1);
        }
    }
}