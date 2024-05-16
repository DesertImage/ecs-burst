using DesertImage.ECS;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Vehicle
{
    public struct EngineSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Vehicle>()
                .With<Engine>()
                .With<Gas>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var engines = _group.GetComponents<Engine>();
            var vehicles = _group.GetComponents<Vehicle>();
            var gases = _group.GetComponents<Gas>();

            foreach (var entityId in _group)
            {
                ref var engine = ref engines.Get(entityId);
                // var vehicle = vehicles.Read(entityId);
                var gas = gases.Read(entityId).Value;

                engine.Torque = math.lerp
                (
                    engine.BackTorque,
                    engine.TorqueCurve.Evaluate(engine.Rpm) * gas,
                    gas
                );

                var angularAcceleration = engine.Torque / engine.Intertia;
                angularAcceleration *= context.DeltaTime;

                engine.AngularVelocity = math.clamp
                (
                    engine.AngularVelocity + angularAcceleration,
                    engine.IdleRpm.RpmToRadiansPerSecond(),
                    engine.MaxRpm.RpmToRadiansPerSecond()
                );

                engine.Rpm = engine.AngularVelocity.RadiansPerSecondToRpm();
                engine.RelativeRpm = engine.Rpm / engine.MaxRpm;

                Debug.Log
                (
                    $"<color=red>[Engine]</color> Torque: {engine.Torque}; RPM: {engine.Rpm} velocitys: {engine.AngularVelocity};"
                );

                // var engineTorque =
                //     engine.EvaluateTorque
                //     (
                //         engine.TransmitWheelRotationSpeedToEngineRpm(vehicle.MaxWheelsAngularVelocity)
                //     )
                //     * gas;
                //
                // var wheelsTorque = engine.TransmitEngineTorqueToWheelTorque(engineTorque);
                //
                // for (var i = vehicle.Wheels.Count - 1; i >= 0; i--)
                // {
                //     vehicle.Wheels[i].Replace(new Torque { Value = wheelsTorque });
                // }
            }
        }
    }
}