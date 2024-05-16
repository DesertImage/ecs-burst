using DesertImage.ECS;

namespace Game.Vehicle
{
    public struct WheelRotationSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Wheel>()
                // .With<Suspension>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var wheels = _group.GetComponents<Wheel>();

            foreach (var entityId in _group)
            {
                ref var wheel = ref wheels.Get(entityId);
                wheel.RadianRotation += wheel.AngularVelocity * context.DeltaTime;
            }
        }
    }
}