namespace DesertImage.ECS
{
    public struct TransformToEntitySystem : IInitSystem, IExecuteSystem
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

                entity.Replace(new Position { Value = transform.position });
                entity.Replace(new LocalPosition { Value = transform.localPosition });
                entity.Replace(new Rotation { Value = transform.rotation });
                entity.Replace(new Scale { Value = transform.localScale });
            }
        }
    }
}