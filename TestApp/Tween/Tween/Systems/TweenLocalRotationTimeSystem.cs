using DesertImage.ECS;

namespace Game.Tween
{
    public struct TweenLocalRotationTimeSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<TweenLocalRotation>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var tweens = _group.GetComponents<TweenLocalRotation>();
            foreach (var i in _group)
            {
                ref var tween = ref tweens.Get(i);

                tween.ElapsedTime += context.DeltaTime;
                
                if (tween.ElapsedTime < tween.Time) continue;

                _group.GetEntity(i).Remove<TweenLocalRotation>();
            }
        }
    }
}