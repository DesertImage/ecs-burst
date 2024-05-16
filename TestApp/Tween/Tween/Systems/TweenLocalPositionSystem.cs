using DesertImage.ECS;
using Unity.Mathematics;

namespace Game.Tween
{
    public struct TweenLocalPositionSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<LocalPosition>()
                .With<TweenLocalPosition>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var localPositions = _group.GetComponents<LocalPosition>();
            var tweenLocalPositions = _group.GetComponents<TweenLocalPosition>();

            foreach (var i in _group)
            {
                var tween = tweenLocalPositions[i];

                localPositions.Get(i).Value = math.lerp
                (
                    tween.Start,
                    tween.End,
                    Easing.GetEase(tween.Ease, tween.ElapsedTime / tween.Time)
                );
            }
        }
    }
}