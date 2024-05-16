using DesertImage.ECS;

namespace Game
{
    public struct ParentToOriginSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _positionsGroup;
        private EntitiesGroup _rotationsGroup;

        public void Initialize(in World world)
        {
            _positionsGroup = Filter.Create(world)
                .With<Parent>()
                .With<OriginPosition>()
                .Find();

            _rotationsGroup = Filter.Create(world)
                .With<Parent>()
                .With<OriginRotation>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var parents = _positionsGroup.GetComponents<Parent>();
            var originPositions = _positionsGroup.GetComponents<OriginPosition>();

            foreach (var entityId in _positionsGroup)
            {
                var parent = parents[entityId].Value;
                if (!parent.Value) continue;
                originPositions.Get(entityId).Value = parent.Value.position;
            }

            parents = _rotationsGroup.GetComponents<Parent>();
            var originRotations = _rotationsGroup.GetComponents<OriginRotation>();

            foreach (var entityId in _rotationsGroup)
            {
                var parent = parents[entityId].Value;
                if (!parent.Value) continue;
                originRotations.Get(entityId).Value = parent.Value.rotation.eulerAngles;
            }
        }
    }
}