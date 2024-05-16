using DesertImage.ECS;
using Game.Tween;
using Unity.Mathematics;

namespace Game
{
    public struct HandCardAlignSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<HandCard>()
                .With<HandCardAlignTag>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var handCards = _group.GetComponents<HandCard>();

            var hand = context.World.ReadStatic<Hand>();
            var cardsCount = hand.Count;

            const float tau = math.PI * 2;
            const float cardWidth = .03f;

            var totalWidth = cardWidth * hand.Count;
            var halfTotalWidth = totalWidth * .5f;
            var step = totalWidth / cardsCount;

            const float radius = 3f;
            const float radiansCenter = .25f;

            foreach (var entityId in _group)
            {
                ref var handCard = ref handCards.Get(entityId);

                var index = handCard.OrderPosition;

                var radians = step * (index + .5f) - halfTotalWidth + radiansCenter;
                var radiansOffset = radiansCenter - radians;

                var vertical = math.sin(radians * tau);
                var horizontal = math.cos(radians * tau) * radius;

                var position = new float3(horizontal, vertical, index * .01f);

                handCard.AlignPosition = position;

                var entity = _group.GetEntity(entityId);

                entity.TweenLocalPosition(position, 1f);
                entity.TweenLocalRotation(new float3(0f, 0f, -radiansOffset * 120f), 1f);
                entity.TweenScale(1f, .8f);
            }
        }
    }
}