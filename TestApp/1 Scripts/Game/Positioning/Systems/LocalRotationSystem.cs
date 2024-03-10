using DesertImage.ECS;

namespace Game
{
    public struct LocalRotationSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<OriginRotation>()
                .With<LocalRotation>()
                .With<Rotation>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var origins = _group.GetComponents<OriginRotation>();
            var rotations = _group.GetComponents<Rotation>();
            var localRotations = _group.GetComponents<LocalRotation>();

            foreach (var entityId in _group)
            {
                rotations.Get(entityId).Value = origins[entityId].Value + localRotations[entityId].Value;
            }
        }
    }
}