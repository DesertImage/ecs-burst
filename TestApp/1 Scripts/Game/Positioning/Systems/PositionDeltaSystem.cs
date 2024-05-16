using DesertImage.ECS;

namespace Game
{
    public struct PositionDeltaSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Position>()
                .With<PositionDelta>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var positions = _group.GetComponents<Position>();
            var positionDeltas = _group.GetComponents<PositionDelta>();

            foreach (var entityId in _group)
            {
                var position = positions.Read(entityId).Value;
                ref var positionDelta = ref positionDeltas.Get(entityId);

                positionDelta.Value = positionDelta.Last - position;
                positionDelta.Last = position;
            }
        }
    }
}