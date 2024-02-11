namespace DesertImage.ECS
{
    public interface IPhysicsSystem : ISystem
    {
        void Execute(Entity entity, float deltaTime);
    }
}