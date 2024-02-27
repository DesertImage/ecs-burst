namespace DesertImage.ECS
{
    public interface IModuleProvider
    {
        T Get<T>();
    }
}