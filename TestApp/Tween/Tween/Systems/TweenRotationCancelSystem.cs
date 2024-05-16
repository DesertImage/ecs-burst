using DesertImage.ECS;

namespace Game.Tween
{
    public struct TweenRotationCancelSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _cancelAllGroup;
        private EntitiesGroup _cancelRotationGroup;

        public void Initialize(in World world)
        {
            _cancelAllGroup = Filter.Create(world)
                .With<TweenRotation>()
                .With<TweenCancelAll>()
                .Find();

            _cancelRotationGroup = Filter.Create(world)
                .With<TweenRotation>()
                .With<TweenRotationCancel>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            foreach (var i in _cancelAllGroup)
            {
                _cancelAllGroup.GetEntity(i).Remove<TweenRotation>();
            }

            foreach (var i in _cancelRotationGroup)
            {
                _cancelRotationGroup.GetEntity(i).Remove<TweenRotation>();
            }
        }
    }
}