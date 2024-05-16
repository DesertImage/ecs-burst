using DesertImage.ECS;
using Game.DragAndDrop;

namespace Game
{
    public struct HandCardReturnToHandSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<HandCard>()
                .With<DropTag>()
                .None<PlayedTag>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            foreach (var entityId in _group)
            {
                var entity = _group.GetEntity(entityId);
                entity.Remove<HoverBlocked>();
                entity.Replace<HandCardAlignTag>();
            }
        }
    }
}