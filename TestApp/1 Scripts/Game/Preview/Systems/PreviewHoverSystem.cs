using DesertImage.ECS;

namespace Game
{
    public struct PreviewHoverSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _hoverGroup;

        public void Initialize(in World world)
        {
            _hoverGroup = Filter.Create(world)
                .With<Hover>()
                .None<InPreview>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            foreach (var entityId in _hoverGroup)
            {
                var entity = _hoverGroup.GetEntity(entityId);
                entity.Replace<InPreview>();
                entity.Replace<PreviewTag>();
            }
        }
    }
}