using DesertImage.ECS;
using Game.DragAndDrop;
using Game.Input;
using Game.Tween;
using UnityEngine;

namespace Game
{
    public struct HandRemoveCardSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;
        private EntitiesGroup _cardsGroup;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<HandRemoveCard>()
                .Find();

            _cardsGroup = Filter.Create(world)
                .With<HandCard>()
                .None<HandRemoveCard>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            ref var hand = ref context.World.GetStatic<Hand>();

            var handRemoveCards = _cardsGroup.GetComponents<HandRemoveCard>();
            var handCards = _cardsGroup.GetComponents<HandCard>();

            foreach (var i in _group)
            {
                var handRemoveCard = handRemoveCards[i];
                hand.Count--;

                foreach (var j in _cardsGroup)
                {
                    ref var handCard = ref handCards.Get(j);

                    if (handCard.OrderPosition >= handRemoveCard.OrderNumber) handCard.OrderPosition--;

                    _group.GetEntity(j).Replace<HandCardAlignTag>();
                }
            }
        }
    }
}