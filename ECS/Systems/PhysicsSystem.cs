namespace DesertImage.ECS
{
    public abstract class PhysicsSystem : SystemBase, IPhysicsSystem
    {
        public abstract Matcher Matcher { get; }

        public abstract void Execute(Entity entity, float deltaTime);
    }
}