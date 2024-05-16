using DesertImage.ECS;
using Game.DragAndDrop;
using Game.Input;
using Game.Tween;
using UnityEngine;

namespace Game
{
    public struct HandCardPlaySystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<HandCard>()
                .With<DropTag>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var mouseRelativePosition = context.World.ReadStatic<MouseRelativePosition>();

            var handCards = _group.GetComponents<HandCard>();

            foreach (var entityId in _group)
            {
                var entity = _group.GetEntity(entityId);

                if (mouseRelativePosition.Value.y <= .5f) return;

                entity.Replace(new HandRemoveCard { OrderNumber = handCards[entityId].OrderPosition });
                entity.Replace<PlayedTag>();
            }
        }
    }
}