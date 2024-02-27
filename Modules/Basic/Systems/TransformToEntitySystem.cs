namespace DesertImage.ECS
{
    public struct TransformToEntitySystem : IExecuteSystem
    {
        public Matcher Matcher => MatcherBuilder.Create()
            .With<View>()
            .With<Position>()
            .With<Rotation>()
            .With<Scale>()
            .Build();

        public void Execute(Entity entity, World world, float deltaTime)
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