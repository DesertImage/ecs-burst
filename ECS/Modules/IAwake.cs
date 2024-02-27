namespace DesertImage.ECS
{
    public interface IAwake
    {
        public void OnAwake(in World world);
    }
}