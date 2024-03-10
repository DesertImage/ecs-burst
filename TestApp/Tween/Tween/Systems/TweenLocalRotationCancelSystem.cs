using DesertImage.ECS;

namespace Game.Tween
{
    public struct TweenLocalRotationCancelSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _cancelAllGroup;
        private EntitiesGroup _cancelLocalRotationGroup;

        public void Initialize(in World world)
        {
            _cancelAllGroup = Filter.Create(world)
                .With<TweenLocalRotation>()
                .With<TweenCancelAll>()
                .Find();

            _cancelLocalRotationGroup = Filter.Create(world)
                .With<TweenLocalRotation>()
                .With<TweenLocalRotationCancel>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            foreach (var i in _cancelAllGroup)
            {
                _cancelAllGroup.GetEntity(i).Remove<TweenLocalRotation>();
            }

            foreach (var i in _cancelLocalRotationGroup)
            {
                _cancelLocalRotationGroup.GetEntity(i).Remove<TweenLocalRotation>();
            }
        }
    }
}