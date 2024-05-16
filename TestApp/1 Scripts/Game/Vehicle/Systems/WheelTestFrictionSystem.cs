using DesertImage.ECS;
using Game.Physics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Vehicle
{
    public struct WheelTestFrictionSystem : IInitSystem, IExecuteSystem
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
                var entity = _group.GetEntity(entityId);

                ref var wheel = ref wheels.Get(entityId);
                ref var friction = ref wheelFrictions.Get(entityId);

                var velocity = wheelVelocitites.Read(entityId);
                var suspensionForce = suspensions.Read(entityId).Force;
                var rigidbody = physicalObjects.Read(entityId).Rigidbody.Value;
                var hit = groundContacts.Read(entityId).Value;
                var rotation = rotations.Read(entityId).Value;

                if (suspensionForce <= 0f) continue;

                var wheelRadius = wheel.Radius;

                var lateralVelocity = velocity.Side;
                var lateralVelocitySign = math.sign(lateralVelocity);
                var lateralVelocityAbs = lateralVelocitySign * lateralVelocity;

                var wheelDelta = wheel.AngularVelocity.AngularToLinear(wheelRadius) - velocity.Forward;
                var longVelocity = wheelDelta;
                var longVelocitySign = math.sign(longVelocity);
                var longVelocityAbs = longVelocitySign * longVelocity;

                // var lateralSlip = math.unlerp
                // (
                //     friction.SideCurve.TimeRange.x,
                //     friction.SideCurve.TimeRange.y,
                //     lateralVelocityAbs
                // );
                //
                // var longSlip = math.unlerp
                // (
                //     friction.ForwardCurve.TimeRange.x,
                //     friction.ForwardCurve.TimeRange.y,
                //     longVelocityAbs
                // );

                friction.Slips = new float2
                (
                    // math.clamp(lateralVelocity, -1f, 1f),
                    // math.clamp(longVelocity, -2f, 2f)
                    lateralVelocity,
                    longVelocity
                );

                var length = math.length(friction.Slips);
                if (length > 1f)
                {
                    friction.Slips = math.normalize(friction.Slips);
                    length = 1f;
                }

                friction.CombinedSlip = length;

                //lateral
                var lateralDirection = math.mul(rotation, math.right());

                // var lateralFrictionRate = 1f;
                // var lateralFrictionRate = friction.Combined.x;
                var lateralFrictionRate = friction.FrictionCurve.Evaluate
                (
                    math.lerp
                    (
                        friction.FrictionCurve.TimeRange.x,
                        friction.FrictionCurve.TimeRange.y,
                        friction.CombinedSlip
                    )
                );

                var tireForce = lateralFrictionRate * friction.Slips * suspensionForce;

                Debug.Log
                (
                    $"<color=green>[Friction]</color> {entityId} tireForce: {tireForce}"
                );
                
                // var lateralFrictionRate = friction.Combined.x;
                // var lateralFrictionValue = suspensionForce * lateralFrictionRate;
                var lateralForce =  -lateralVelocitySign * tireForce.x * lateralDirection;

                //longitudinal
                var longDirection = math.normalize(math.cross(lateralDirection, hit.normal));

                // var longFrictionRate = 1f;
                // var longFrictionRate = math.abs(friction.Combined.y);
                var longFrictionRate = friction.FrictionCurve.Evaluate
                (
                    math.lerp
                    (
                        friction.FrictionCurve.TimeRange.x,
                        friction.FrictionCurve.TimeRange.y,
                        friction.CombinedSlip
                    )
                );

                // var longFrictionRate = friction.Combined.y;

                Debug.Log(
                    $"<color=green>[Friction]</color> {entityId} combined: {friction.Slips}. Slip: {friction.CombinedSlip}");

                var longFrictionValue = suspensionForce * longFrictionRate;
                // var longFrictionValue = 0f;
                var longForce = longVelocitySign * tireForce.y * longDirection;

                Debug.Log
                (
                    $"<color=green>[Friction]</color> {entityId} velocitys: {new float2(lateralVelocityAbs, longVelocityAbs)}"
                );

                // Debug.Log
                // (
                    // $"<color=green>[Friction]</color> {entityId} friction: {new float2(lateralFrictionValue, longFrictionValue)}"
                // );

                if (longVelocityAbs > 1f)
                {
                    wheel.AngularVelocity -= longVelocitySign *
                                             (longVelocityAbs * longFrictionRate).LinearToAngular(wheel.Radius);
                }
                else
                {
                    wheel.AngularVelocity -= (wheelDelta * longFrictionRate).LinearToAngular(wheel.Radius);
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