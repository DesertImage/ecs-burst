using DesertImage.ECS;
using Game.Physics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Vehicle
{
    public struct WheelCombinedNewFrictionSystem : IInitSystem, IExecuteSystem
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
                

                var lateralSlip = math.unlerp(0f, 20f, lateralVelocityAbs);
                var longSlip = math.unlerp(0f, 20f, longVelocityAbs);

                friction.CombinedSlip = math.max(lateralSlip, longSlip);
                
                friction.Slips = new float2
                (
                    math.clamp(lateralVelocity * friction.CorneringStiffness, -1f, 1f),
                    math.clamp(longVelocity * friction.ForwardStiffness, -2f, 2f)
                );

                Debug.Log
                (
                    $"<color=green>[Friction]</color> {entityId} friction rate: {friction.Slips}"
                );

                var frictionRate = friction.FrictionCurve.Evaluate(math.lerp(0f, 20f, friction.CombinedSlip));


                //lateral
                var lateralDirection = math.mul(rotation, math.right());

                Debug.Log
                (
                    $"<color=green>[Friction]</color> {entityId} tireForce: {frictionRate * friction.Slips}. combined slip: {friction.CombinedSlip}"
                );

                Debug.Log
                (
                    $"<color=green>[Friction]</color> {entityId} friction rate: {frictionRate}"
                );

                var lateralForce = -lateralVelocitySign * frictionRate * suspensionForce * lateralDirection;

                //longitudinal
                var longDirection = math.normalize(math.cross(lateralDirection, hit.normal));

                frictionRate = .8f;
                var longForce = wheelDelta * frictionRate * longDirection;

                if (longVelocityAbs > 1f)
                {
                    wheel.AngularVelocity -=
                        longVelocitySign * (longVelocityAbs * frictionRate).LinearToAngular(wheel.Radius);
                }
                else
                {
                    wheel.AngularVelocity -= (wheelDelta * frictionRate).LinearToAngular(wheel.Radius);
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