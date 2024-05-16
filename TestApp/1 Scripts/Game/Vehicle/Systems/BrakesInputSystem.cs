using DesertImage.ECS;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Vehicle
{
    public struct BrakesInputSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;
        private EntitiesGroup _vehicleToWheelsGroup;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Brakes>()
                .With<BrakesInput>()
                .Find();

            _vehicleToWheelsGroup = Filter.Create(world)
                .With<Vehicle>()
                .With<Brakes>()
                .With<BrakesInput>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var brakesPool = _group.GetComponents<Brakes>();
            var vehicles = _vehicleToWheelsGroup.GetComponents<Vehicle>();

            foreach (var entityId in _group)
            {
                ref var brakes = ref brakesPool.Get(entityId);

                brakes.Input = -math.min(0f, UnityEngine.Input.GetAxis("Vertical"));
                brakes.HandBrakeInput = UnityEngine.Input.GetKey(KeyCode.Space) ? 1f : 0f;
            }

            foreach (var entityId in _vehicleToWheelsGroup)
            {
                var vehicle = vehicles.Read(entityId);
                for (var i = vehicle.Wheels.Count - 1; i >= 0; i--)
                {
                    var wheelEntity = vehicle.Wheels[i];
                    
                    if(!wheelEntity.Has<Brakes>()) continue;
                    
                    var brakes = brakesPool.Read(entityId);
                    wheelEntity.Replace(brakes);
                }
            }
        }
    }
}