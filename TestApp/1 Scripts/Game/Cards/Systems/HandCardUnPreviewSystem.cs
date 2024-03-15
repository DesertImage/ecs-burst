using DesertImage.ECS;
using Game.DragAndDrop;

namespace Game
{
    public struct HandCardUnPreviewSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<HandCard>()
                .With<LocalPosition>()
                .With<LocalRotation>()
                .With<UnPreviewTag>()
                .None<DropTag>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            foreach (var entityId in _group)
            {
                var entity = _group.GetEntity(entityId);
                entity.Replace<HandCardAlignTag>();
            }
        }
    }
}