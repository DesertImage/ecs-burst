using DesertImage.ECS;

namespace Game.Tween
{
    public struct TweenLocalPositionCancelSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _cancelAllGroup;
        private EntitiesGroup _cancelLocalPositionGroup;

        public void Initialize(in World world)
        {
            _cancelAllGroup = Filter.Create(world)
                .With<TweenLocalPosition>()
                .With<TweenCancelAll>()
                .Find();

            _cancelLocalPositionGroup = Filter.Create(world)
                .With<TweenLocalPosition>()
                .With<TweenLocalPositionCancel>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            foreach (var i in _cancelAllGroup)
            {
                _cancelAllGroup.GetEntity(i).Remove<TweenLocalPosition>();
            }

            foreach (var i in _cancelLocalPositionGroup)
            {
                _cancelLocalPositionGroup.GetEntity(i).Remove<TweenLocalPosition>();
            }
        }
    }
}