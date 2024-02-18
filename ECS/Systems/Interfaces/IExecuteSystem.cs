namespace DesertImage.ECS
{
    public interface IExecuteSystem : IMatchSystem
    {
        void Execute(Entity entity, World world, float deltaTime);
    }
}