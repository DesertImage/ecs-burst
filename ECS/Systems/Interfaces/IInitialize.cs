namespace DesertImage.ECS
{
    public interface IInitialize : ISystem
    {
        void Initialize(in World world);
    }
}