namespace DesertImage.ECS
{
    public interface IInitSystem : ISystem
    {
        void Initialize(in World world);
    }
}