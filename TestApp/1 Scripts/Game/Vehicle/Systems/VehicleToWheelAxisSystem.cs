using DesertImage.ECS;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Vehicle
{
    public struct VehicleToWheelAxisSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _engineGroup;
        private EntitiesGroup _steeringGroup;
        private EntitiesGroup _brakesGroup;

        public void Initialize(in World world)
        {
            _engineGroup = Filter.Create(world)
                .With<Vehicle>()
                .With<Engine>()
                .Find();

            _steeringGroup = Filter.Create(world)
                .With<Vehicle>()
                .With<Steering>()
                .Find();

            _brakesGroup = Filter.Create(world)
                .With<Vehicle>()
                .With<Brakes>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var vehicles = _steeringGroup.GetComponents<Vehicle>();
            var engines = _steeringGroup.GetComponents<Engine>();
            var steerings = _steeringGroup.GetComponents<Steering>();
            var brakes = _steeringGroup.GetComponents<Brakes>();
            var wheelAxises = _steeringGroup.GetComponents<WheelAxis>();

            foreach (var entityId in _engineGroup)
            {
                var vehicle = vehicles.Get(entityId);
                var engine = engines.Read(entityId);

               for (var i = vehicle.WheelAxis.Count - 1; i >= 0; i--)
               {
                   ref var axis = ref wheelAxises.Get(vehicle.WheelAxis[i].Id);
                   axis.Torque = engine.Torque * axis.DriveRatio;
               }
            }
            
            foreach (var entityId in _steeringGroup)
            {
                var vehicle = vehicles.Get(entityId);
                var steering = steerings.Read(entityId);

                for (var i = vehicle.WheelAxis.Count - 1; i >= 0; i--)
                {
                    var axis = vehicle.WheelAxis[i];
                    axis.Replace(steering);
                }
            }
            
            foreach (var entityId in _brakesGroup)
            {
                var vehicle = vehicles.Get(entityId);
                var brake = brakes.Read(entityId);

                for (var i = vehicle.WheelAxis.Count - 1; i >= 0; i--)
                {
                    var axis = vehicle.WheelAxis[i];
                    axis.Replace(brake);
                }
            }
        }
    }
}