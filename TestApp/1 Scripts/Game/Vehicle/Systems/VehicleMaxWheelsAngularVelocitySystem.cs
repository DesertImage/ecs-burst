using DesertImage.ECS;
using Unity.Mathematics;

namespace Game.Vehicle
{
    public struct VehicleMaxWheelsAngularVelocitySystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Vehicle>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var vehicles = _group.GetComponents<Vehicle>();

            foreach (var entityId in _group)
            {
                ref var vehicle = ref vehicles.Get(entityId);
                for (var i = 0; i < vehicle.Wheels.Count; i++)
                {
                    var wheel = vehicle.Wheels[i];
                    var angularVelocity = wheel.Read<Wheel>().AngularVelocity;

                    vehicle.MaxWheelsAngularVelocity = math.max(vehicle.MaxWheelsAngularVelocity, angularVelocity);
                }
            }
        }
    }
}