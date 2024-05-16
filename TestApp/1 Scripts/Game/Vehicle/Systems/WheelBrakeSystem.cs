using DesertImage.ECS;
using Unity.Mathematics;

namespace Game.Vehicle
{
    public struct WheelBrakeSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Wheel>()
                .With<Brakes>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var wheels = _group.GetComponents<Wheel>();
            var brakePool = _group.GetComponents<Brakes>();

            var deltaTime = context.DeltaTime;

            foreach (var entityId in _group)
            {
                ref var wheel = ref wheels.Get(entityId);
                var brakes = brakePool.Read(entityId);

                var handBrakeInput = _group.GetEntity(entityId).Has<HandBrakeWheelAxis>() ? brakes.HandBrakeInput : 0f;

                var brakeTorque =
                    brakes.Torque = brakes.Torque * brakes.Input + brakes.HandBrakeTorque * handBrakeInput;

                if (brakeTorque <= 0) continue;

                // if (brakes.HandBrakeInput > 0f)
                // {
                //     wheel.AngularVelocity = -wheel.AngularVelocity.LinearToAngular(wheel.Radius);
                // }
                // else
                // {
                var wheelInertia = wheel.Inertia;
                var toZeroTorque = -wheel.AngularVelocity * wheelInertia / deltaTime;
                var toZeroTorqueAbs = math.abs(toZeroTorque);
                var usedBrakeTorque = toZeroTorqueAbs < brakeTorque ? toZeroTorqueAbs : brakeTorque;

                wheel.AngularVelocity += math.sign(toZeroTorque) * usedBrakeTorque / wheelInertia * deltaTime;
                // }
            }
        }
    }
}