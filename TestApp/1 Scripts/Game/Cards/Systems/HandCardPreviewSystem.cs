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
                .None<HoverBlocked>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var handCards = _group.GetComponents<HandCard>();
            var localPositions = _group.GetComponents<LocalPosition>();

            foreach (var entityId in _group)
            {
                var handCard = handCards.Get(entityId);
                var entity = _group.GetEntity(entityId);

                var previewPosition = localPositions.Read(entityId).Value;
                previewPosition.y = handCard.AlignPosition.y + .3f;
                previewPosition.z = handCard.AlignPosition.z - 1f;

                entity.TweenLocalPosition(previewPosition, Duration);
                entity.TweenLocalRotation(float3.zero, Duration);
                entity.TweenScale(1.2f, Duration);
            }
        }
    }
}