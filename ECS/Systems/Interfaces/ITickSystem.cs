namespace DesertImage.ECS
{
    public interface ITickSystem : ISystem
    {
        void Execute(float deltaTime);
    }
}