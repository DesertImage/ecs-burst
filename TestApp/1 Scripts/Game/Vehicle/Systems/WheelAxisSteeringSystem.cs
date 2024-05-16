using DesertImage.ECS;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Vehicle
{
    public struct WheelAxisSteeringSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;
        private EntitiesGroup _toWheelsGroup;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Vehicle>()
                .With<Steering>()
                .Find();

            _toWheelsGroup = Filter.Create(world)
                .With<WheelAxis>()
                .With<SteeringWheelAxis>()
                .With<Steering>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var vehicles = _toWheelsGroup.GetComponents<Vehicle>();
            var wheelAxises = _toWheelsGroup.GetComponents<WheelAxis>();
            var steerings = _toWheelsGroup.GetComponents<Steering>();

            foreach (var entityId in _group)
            {
                var vehicle = vehicles.Read(entityId);
                var steering = steerings.Read(entityId);

                for (var i = 0; i < vehicle.WheelAxis.Count; i++)
                {
                    var axis = vehicle.WheelAxis[i];
                    axis.Replace(steering);
                }
            }

            foreach (var entityId in _toWheelsGroup)
            {
                var wheelAxis = wheelAxises.Read(entityId);
                var steering = steerings.Read(entityId);

                wheelAxis.Left.Replace(steering);
                wheelAxis.Right.Replace(steering);
            }
        }
    }
}