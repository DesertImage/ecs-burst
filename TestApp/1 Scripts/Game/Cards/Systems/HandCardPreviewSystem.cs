using DesertImage.ECS;
using Game.Tween;
using Unity.Mathematics;

namespace Game
{
    public struct HandCardPreviewSystem : IInitSystem, IExecuteSystem
    {
        private const float Duration = .3f;

        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<HandCard>()
                .With<PreviewTag>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var localPositions = _group.GetComponents<LocalPosition>();

            foreach (var entityId in _group)
            {
                var entity = _group.GetEntity(entityId);

                var previewPosition = localPositions.Read(entityId).Value;
                previewPosition.y = 1f;
                previewPosition.z = Duration;

                entity.TweenLocalPosition(previewPosition, Duration);
                entity.TweenLocalRotation(float3.zero, Duration);
                entity.TweenScale(1.2f, Duration);
            }
        }
    }
}