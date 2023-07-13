namespace DesertImage.ECS
{
    public interface IPhysicsSystem : IMatchSystem
    {
        void Execute(Entity entity, float deltaTime);
    }
}