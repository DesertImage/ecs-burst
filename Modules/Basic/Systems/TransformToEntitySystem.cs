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
                .Build();
        }

        public unsafe void Execute(SystemsContext* context)
        {
            foreach (var entity in _group)
            {
                var view = entity.Read<View>().Value.Value;
                var transform = view.transform;

                entity.Replace(new Position { Value = transform.position });
                entity.Replace(new LocalPosition { Value = transform.localPosition });
                entity.Replace(new Rotation { Value = transform.rotation.eulerAngles });
                entity.Replace(new Scale { Value = transform.localScale });
            }
        }
    }
}