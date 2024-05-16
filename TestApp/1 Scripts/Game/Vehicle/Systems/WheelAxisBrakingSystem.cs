using DesertImage.ECS;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Vehicle
{
    public struct WheelAxisBrakingSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _toWheelsGroup;

        public void Initialize(in World world)
        {
            _toWheelsGroup = Filter.Create(world)
                .With<WheelAxis>()
                .With<HandBrakeWheelAxis>()
                .With<Brakes>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var wheelAxises = _toWheelsGroup.GetComponents<WheelAxis>();
            var brakes = _toWheelsGroup.GetComponents<Brakes>();

            foreach (var entityId in _toWheelsGroup)
            {
                var wheelAxis = wheelAxises.Read(entityId);
                var brake = brakes.Read(entityId);

                wheelAxis.Left.Replace(brake);
                wheelAxis.Right.Replace(brake);
            }
        }
    }
}