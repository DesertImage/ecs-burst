using DesertImage.ECS;
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
                .With<HandCardAlign>()
                .With<LocalPosition>()
                .Find();
        }

        public unsafe void Execute(ref SystemsContext context)
        {
            var handCards = _group.GetComponents<HandCard>();
            var localPositions = _group.GetComponents<LocalPosition>();

            var hand = context.World->GetStatic<Hand>();
            var cardCount = hand.Count;

            var half = cardCount / 2;
            var isEven = cardCount % 2 == 0;

            for (var i = _group.Count - 1; i >= 0; i--)
            {
                var handCard = handCards[i];

                var index = handCard.OrderPosition;
                var spacing = hand.Spacing;

                var newPosX = (index - half) * spacing + (isEven ? spacing * .5f : 0f);

                localPositions.Get(i).Value = new float3(newPosX, 0f, 0f);
            }
        }
    }
}