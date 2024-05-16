using DesertImage.ECS;
using Game.Physics;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Vehicle
{
    public struct WheelFrictionSystem : IInitSystem, IExecuteSystem
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

                // var mass = rigidbody.mass * .25f;

                var maxFriction = suspensionForce * friction.GroundGrip;

                //lateral
                var lateralDirection = math.mul(rotation, math.right());
                var lateralVelocity = velocity.Side;
                // var lateralVelocitySign = math.sign(lateralVelocity);
                // var lateralVelocityAbs = lateralVelocity * lateralVelocitySign;
                // var lateralFriction = friction.SideCurve.Evaluate(math.saturate(lateralVelocityAbs));
                // var lateralFriction = 1.6f;

                var lateralSlipNormalized = math.clamp
                (
                    lateralVelocity * friction.CorneringStiffness * -1f,
                    -1,
                    1
                );
                // lateralFriction += .3f;

                Debug.Log($"lateralSlip: {lateralVelocity} lateralSlipNorm: {lateralSlipNormalized}");

                // var lateralTimeRange = friction.SideCurve.TimeRange;

                // var lateralFriction = friction.SideCurve.Evaluate
                // (
                // math.lerp(lateralTimeRange.x, lateralTimeRange.y, lateralVelocityAbs)
                // );

                // var lateralForce = (-lateralVelocitySign * suspensionForce * lateralFriction *
                // Bias(math.saturate(lateralVelocityAbs), -1)) *
                // lateralDirection;

                //forward
                var longSlip = wheel.AngularVelocity.AngularToLinear(wheel.Radius) - velocity.Forward;
                var wheelDeltaVelocitySign = math.sign(longSlip);
                // var wheelDeltaVelocityAbs = wheelDeltaVelocitySign * wheelDeltaVelocity;

                // wheel.
                // wheel.AngularVelocity -= longSlip.LinearToAngular(wheel.Radius);

                // var forwardFriction = friction.ForwardCurve.Evaluate(wheelDeltaVelocity);
                var forwardFriction = 1f;
                var forwardFrictionForce = forwardFriction * longSlip;
                var forwardFrictionTorque = (forwardFrictionForce).LinearToAngular(wheel.Radius);

                var forwardSlipNormalized = 0f;

                if (longSlip * velocity.Forward > 0f)
                {
                    //traction
                    var driveTorque = 0f;
                    if (entity.Has<DriveWheel>())
                    {
                        var driveWheel = driveWheels.Read(entityId);
                        driveTorque = driveWheel.Torque;
                    }

                    var traction = driveTorque.LinearToAngular(wheel.Radius);
                    forwardSlipNormalized = traction / (suspensionForce * friction.GroundGrip);
                }
                else
                {
                    //friction
                    forwardSlipNormalized = longSlip * friction.ForwardStiffness;
                }

                forwardSlipNormalized = math.clamp
                (
                    forwardSlipNormalized,
                    -1f,
                    1f
                );

                // forwardSlipNormalized = 0f;

                friction.Slips = new float2(lateralSlipNormalized, forwardSlipNormalized);
                // friction.Combined = math.normalize(new float2(lateralSlipNormalized, forwardSlipNormalized));
                friction.CombinedSlip = math.length(friction.Slips);

                Debug.Log(
                    $"<color=green>[Friction]</color> LtSlip: {lateralSlipNormalized}, LgSlip: {forwardSlipNormalized} CombinedSlip: {friction.CombinedSlip}");

                var combinedFriction = friction.FrictionCurve.Evaluate(friction.CombinedSlip);
                var tireForceNormalized = friction.Slips * combinedFriction;
                var tireForce = tireForceNormalized * math.max(0f, maxFriction);

                // friction.

                // var maxFriction = suspensionForce * friction.GroundGrip;

                // var frictionForce = 

                // var forwardTimeRange = friction.ForwardCurve.TimeRange;
                // var forwardFriction = math.saturate
                // (
                // math.unlerp(forwardTimeRange.x, forwardTimeRange.y, wheelDeltaVelocityAbs)
                // );

                // var forwardFrictionForce = -wheelDeltaVelocitySign * forwardFriction *
                //                            suspensionForce *
                //                            Bias(math.saturate(wheelDeltaVelocityAbs), -1);

                var forwardDirection = math.normalize(math.cross(lateralDirection, hit.normal));
                // var forwardForce = -wheelDeltaVelocitySign * forwardFrictionForce * forwardDirection;
                // var forwardForce = 0f;

                var forwardForce = tireForce.y * Bias(math.saturate(forwardSlipNormalized), -1) * forwardDirection;
                var lateralForce = tireForce.x * /*Bias(math.saturate(tireForce.x), -1) **/ lateralDirection;

                // wheel.AngularVelocity -= forwardFrictionTorque / wheel.Inertia;

                rigidbody.AddForceAtPosition
                (
                    (forwardForce + lateralForce) * deltaTime,
                    hit.point,
                    ForceMode.Impulse
                );

                // var toNeutralForce = (-wheelDeltaVelocity.LinearToAngular(wheel.Radius) * wheel.Inertia / deltaTime)
                // .TorqueToForce(wheel.Radius);

                // var usedForceValue = math.abs(toNeutralForce) > math.abs(forwardFrictionForce)
                // ? forwardFrictionForce
                // : toNeutralForce;

                // wheel.AngularVelocity -= usedForceValue.ForceToTorque(wheel.Radius) / wheel.Inertia * deltaTime;
            }
        }

        private static float Bias(float x, float bias)
        {
            var k = Mathf.Pow(1 - bias, 3);
            return (x * k) / (x * k - x + 1);
        }
    }
}