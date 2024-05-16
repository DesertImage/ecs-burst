using DesertImage.ECS;
using Game.DragAndDrop;
using Game.Tween;

namespace Game
{
    public struct HandCardDragSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<HandCard>()
                .With<DragStartTag>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            foreach (var entityId in _group)
            {
                var entity = _group.GetEntity(entityId);

                entity.Replace<HoverBlocked>();
                entity.TweenScale(1f, .2f);
            }
        }
    }
}