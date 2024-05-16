using DesertImage.ECS;

namespace Game.Vehicle
{
    public struct SteeringInputSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<SteeringInput>()
                .With<Steering>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var steerings = _group.GetComponents<Steering>();

            foreach (var entityId in _group)
            {
                ref var steering = ref steerings.Get(entityId);
                steering.Value = UnityEngine.Input.GetAxis("Horizontal");
            }
        }
    }
}