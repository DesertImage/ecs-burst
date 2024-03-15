using DesertImage.ECS;

namespace Game
{
    public struct PreviewUnhoverSystem : IInitSystem, IExecuteSystem
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<Hover>()
                .With<Unhover>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            foreach (var entityId in _group)
            {
                var entity = _group.GetEntity(entityId);
                entity.Remove<Hover>();

                if (entity.Has<InPreview>())
                {
                    entity.Remove<InPreview>();
                }

                entity.Remove<Unhover>();

                entity.Replace<UnPreviewTag>();
            }
        }
    }
}