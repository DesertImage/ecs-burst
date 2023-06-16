namespace DesertImage.ECS
{
    public class RemoveComponentSystem<T> : ExecuteSystem where T : struct
    {
        public override Matcher Matcher => MatcherBuilder.Create().With<T>().Build();

        public override void Execute(Entity entity, float deltaTime) => entity.Remove<T>();
    }
}