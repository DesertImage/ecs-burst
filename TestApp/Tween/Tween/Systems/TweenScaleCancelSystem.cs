using DesertImage.ECS;

namespace Game.Tween
{
    public struct TweenScaleCancelSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _cancelAllGroup;
        private EntitiesGroup _cancelScaleGroup;

        public void Initialize(in World world)
        {
            _cancelAllGroup = Filter.Create(world)
                .With<TweenScale>()
                .With<TweenCancelAll>()
                .Find();

            _cancelScaleGroup = Filter.Create(world)
                .With<TweenScale>()
                .With<TweenScaleCancel>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            foreach (var i in _cancelAllGroup)
            {
                _cancelAllGroup.GetEntity(i).Remove<TweenScale>();
            }

            foreach (var i in _cancelScaleGroup)
            {
                _cancelScaleGroup.GetEntity(i).Remove<TweenScale>();
            }
        }
    }
}