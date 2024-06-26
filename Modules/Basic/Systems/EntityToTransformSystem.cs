namespace DesertImage.ECS
{
    public struct EntityToTransformSystem : IInitialize, IExecute
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<View>()
                .With<Position>()
                .With<Rotation>()
                .With<Scale>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            foreach (var i in _group)
            {
                var entity = _group.GetEntity(i);
                var view = entity.Read<View>().Value.Value;
                var transform = view.transform;

                transform.position = entity.Read<Position>().Value;
                transform.rotation = entity.Read<Rotation>().Value;
                transform.localScale = entity.Read<Scale>().Value;
            }
        }
    }
}