using DesertImage.ECS;

namespace Game.Tween
{
    public struct TweenRotationTimeSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<TweenRotation>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var tweens = _group.GetComponents<TweenRotation>();
            foreach (var i in _group)
            {
                ref var tween = ref tweens.Get(i);

                tween.ElapsedTime += context.DeltaTime;

                if (tween.ElapsedTime < tween.Time) continue;

                _group.GetEntity(i).Remove<TweenRotation>();
            }
        }
    }
}