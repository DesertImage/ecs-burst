using DesertImage.ECS;
using UnityEngine;

namespace Game.Tween
{
    public struct TweenLocalPositionTimeSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<TweenLocalPosition>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var tweens = _group.GetComponents<TweenLocalPosition>();

            foreach (var i in _group)
            {
                var entity = _group.GetEntity(i);

                ref var tween = ref tweens.Get(i);

                // Debug.Log($"Entity: {i}. Elapsed: {tween.ElapsedTime}. Target: {tween.Time}");
                
                tween.ElapsedTime += context.DeltaTime;

                if (tween.ElapsedTime < tween.Time) continue;

                entity.Remove<TweenLocalPosition>();
            }
        }
    }
}