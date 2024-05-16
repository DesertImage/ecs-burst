using DesertImage.ECS;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Tween
{
    public struct TweenLocalRotationSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<LocalRotation>()
                .With<TweenLocalRotation>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var localRotations = _group.GetComponents<LocalRotation>();
            var tweenLocalRotations = _group.GetComponents<TweenLocalRotation>();

            foreach (var i in _group)
            {
                var tween = tweenLocalRotations[i];

                localRotations.Get(i).Value = math.slerp
                (
                    tween.Start,
                    tween.End,
                    Easing.GetEase(tween.Ease, tween.ElapsedTime / tween.Time)
                );
            }
        }
    }
}