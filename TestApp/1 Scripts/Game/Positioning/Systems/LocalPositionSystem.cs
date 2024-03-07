using DesertImage.ECS;

namespace Game
{
    public struct LocalPositionSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<OriginPosition>()
                .With<LocalPosition>()
                .With<Position>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var originPositions = _group.GetComponents<OriginPosition>();
            var positions = _group.GetComponents<Position>();
            var localPositions = _group.GetComponents<LocalPosition>();

            var count = _group.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                positions.Get(i).Value = originPositions[i].Value + localPositions[i].Value;
            }
        }
    }
}