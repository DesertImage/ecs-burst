using DesertImage.ECS;

namespace Game.Vehicle
{
    public struct WheelAxisTorqueSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;
        private EntitiesGroup _toWheelsGroup;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Vehicle>()
                .With<Engine>()
                .With<Gearbox>()
                .Find();

            _toWheelsGroup = Filter.Create(world)
                .With<WheelAxis>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var vehicles = _toWheelsGroup.GetComponents<Vehicle>();
            var engines = _toWheelsGroup.GetComponents<Engine>();
            var gearboxes = _toWheelsGroup.GetComponents<Gearbox>();
            var wheelAxises = _toWheelsGroup.GetComponents<WheelAxis>();

            foreach (var entityId in _group)
            {
                var vehicle = vehicles.Read(entityId);
                var engine = engines.Read(entityId);
                var totalGearRatio = gearboxes.Read(entityId).TotalGearRatio;

                for (var i = 0; i < vehicle.WheelAxis.Count; i++)
                {
                    var axisEntity = vehicle.WheelAxis[i];

                    ref var axis = ref wheelAxises.Get(axisEntity.Id);
                    axis.Torque = engine.Torque * totalGearRatio * axis.DriveRatio;
                }
            }

            foreach (var entityId in _toWheelsGroup)
            {
                var wheelAxis = wheelAxises.Read(entityId);

                var torque = wheelAxis.Torque * .5f; //divided by 2 because split between wheels

                wheelAxis.Left.Replace(new DriveWheel { Torque = torque });
                wheelAxis.Right.Replace(new DriveWheel { Torque = torque });
            }
        }
    }
}