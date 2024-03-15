using DesertImage.ECS;

namespace Game.Tween
{
    public struct TweenPositionCancelSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _cancelAllGroup;
        private EntitiesGroup _cancelPositionGroup;

        public void Initialize(in World world)
        {
            _cancelAllGroup = Filter.Create(world)
                .With<TweenPosition>()
                .With<TweenCancelAll>()
                .Find();

            _cancelPositionGroup = Filter.Create(world)
                .With<TweenPosition>()
                .With<TweenPositionCancel>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            foreach (var i in _cancelAllGroup)
            {
                _cancelAllGroup.GetEntity(i).Remove<TweenPosition>();
            }

            foreach (var i in _cancelPositionGroup)
            {
                _cancelPositionGroup.GetEntity(i).Remove<TweenPosition>();
            }
        }
    }
}