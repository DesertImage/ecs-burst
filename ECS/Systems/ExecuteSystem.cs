namespace DesertImage.ECS
{
    public abstract class ExecuteSystem : SystemBase, IExecuteSystem
    {
        public abstract Matcher Matcher { get; }

        public abstract void Execute(Entity entity, float delta);
    }
}