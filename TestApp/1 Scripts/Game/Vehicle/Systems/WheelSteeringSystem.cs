using DesertImage.ECS;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Vehicle
{
    public struct WheelSteeringSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;
        // private EntitiesGroup _vehicleToWheelsGroup;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Wheel>()
                // .With<SteeringWheel>()
                .With<Steering>()
                .With<SteeringWheel>()
                .With<View>()
                .Find();

            // _vehicleToWheelsGroup = Filter.Create(world)
            //     .With<Vehicle>()
            //     .With<Steering>()
            //     .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var steerings = _group.GetComponents<Steering>();
            var vehicles = _group.GetComponents<Vehicle>();

            // foreach (var entityId in _vehicleToWheelsGroup)
            // {
            //     var vehicle = vehicles.Read(entityId);
            //     for (var i = vehicle.Wheels.Count - 1; i >= 0; i--)
            //     {
            //         var wheelEntity = vehicle.Wheels[i];
            //         var steering = steerings.Read(entityId);
            //         wheelEntity.Replace(steering);
            //     }
            // }

            // var wheels = _group.GetComponents<Wheel>();
            var steeringWheels = _group.GetComponents<SteeringWheel>();
            // var rotations = _group.GetComponents<Rotation>();
            var views = _group.GetComponents<View>();

            foreach (var entityId in _group)
            {
                // ref var rotation = ref rotations.Get(entityId);
                // ref var wheel = ref wheels.Get(entityId);
                var view = views.Read(entityId);
                var steering = steerings.Read(entityId).Value;
                var steeringWheel = steeringWheels.Read(entityId);

                var transform = view.Value.Value.transform;

                // var transform = wheel.View.Value;

                var localRotation = transform.localRotation.eulerAngles;
                localRotation.y = math.lerp(0f, steeringWheel.MaxAngle, steering);
                transform.localRotation = Quaternion.Euler(localRotation);
            }
        }
    }
}