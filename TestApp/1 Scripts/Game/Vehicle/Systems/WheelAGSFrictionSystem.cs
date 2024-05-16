using DesertImage.ECS;
using Game.Physics;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Vehicle
{
    public struct WheelAGSFrictionSystem : IInitSystem, IExecuteSystem
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
            var driveWheels = _group.GetComponents<DriveWheel>();
            var wheelVelocitites = _group.GetComponents<WheelVelocity>();
            var wheelFrictions = _group.GetComponents<WheelFriction>();
            var suspensions = _group.GetComponents<Suspension>();
            var rotations = _group.GetComponents<Rotation>();
            var physicalObjects = _group.GetComponents<PhysicalObject>();
            var groundContacts = _group.GetComponents<WheelContact>();

            foreach (var entityId in _group)
            {
                var entity = _group.GetEntity(entityId);

                ref var friction = ref wheelFrictions.Get(entityId);
                ref var wheel = ref wheels.Get(entityId);

                var velocity = wheelVelocitites.Read(entityId);
                var suspensionForce = suspensions.Read(entityId).Force;
                var rigidbody = physicalObjects.Read(entityId).Rigidbody.Value;
                var hit = groundContacts.Read(entityId).Value;
                var rotation = rotations.Read(entityId).Value;

                if (suspensionForce <= 0f) continue;

                var lateralDirection = math.mul(rotation, math.right());
                var forwardDirection = math.normalize(math.cross(lateralDirection, hit.normal));

                var lateralVelocity = velocity.Side;
                var lateralVelocitySign = math.sign(lateralVelocity);
                var lateralVelocityAbs = lateralVelocity * lateralVelocitySign;

                var longVelocity = wheel.AngularVelocity.AngularToLinear(wheel.Radius) - velocity.Forward;
                var longVelocitySign = math.sign(longVelocity);
                var longVelocityAbs = longVelocitySign * longVelocity;

                var lateralTimeRange = friction.FrictionCurve.TimeRange;

                var lateralSlip = math.unlerp
                (
                    friction.FrictionCurve.TimeRange.x,
                    friction.FrictionCurve.TimeRange.y,
                    lateralVelocityAbs
                );

                var longSlip = math.unlerp
                (
                    friction.FrictionCurve.TimeRange.x,
                    friction.FrictionCurve.TimeRange.y,
                    longVelocityAbs
                );

                friction.CombinedSlip = math.max(lateralSlip, longSlip);

                Debug.Log
                (
                    $"<color=green>[Friction]</color> {entityId} velocity {new float2(lateralVelocityAbs, longVelocityAbs)} combined slip: {friction.CombinedSlip}"
                );

                var lateralFrictionRate = friction.FrictionCurve.Evaluate
                (
                    math.lerp(lateralTimeRange.x, lateralTimeRange.y, friction.CombinedSlip)
                );

                var lateralFrictionValue = suspensionForce * lateralFrictionRate *
                                           Bias(math.saturate(lateralVelocityAbs), -1);

                var lateralForce = (-lateralVelocitySign * lateralFrictionValue) *
                                   lateralDirection;

                var forwardTimeRange = friction.FrictionCurve.TimeRange;
                var forwardFrictionRate = math.saturate
                (
                    math.lerp(forwardTimeRange.x, forwardTimeRange.y, friction.CombinedSlip)
                );

                var forwardFrictionValue = forwardFrictionRate * suspensionForce;
                var forwardForce = longVelocitySign * forwardFrictionValue * forwardDirection;

                Debug.Log
                (
                    $"<color=green>[Friction]</color> {entityId} rate: {new float2(lateralFrictionRate, forwardFrictionRate)} friction {new float2(lateralFrictionValue, forwardFrictionValue)}"
                );

                rigidbody.AddForceAtPosition
                (
                    (forwardForce + lateralForce) * deltaTime,
                    hit.point,
                    ForceMode.Impulse
                );

                var toNeutralForce = (-longVelocity.LinearToAngular(wheel.Radius) * wheel.Inertia / deltaTime)
                    .TorqueToForce(wheel.Radius);

                var usedForceValue = math.abs(toNeutralForce) > math.abs(forwardFrictionRate)
                    ? forwardFrictionRate
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