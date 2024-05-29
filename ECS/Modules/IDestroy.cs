namespace DesertImage.ECS
{
    public interface IDestroy
    {
        void OnDestroy(in World world);
    }
}