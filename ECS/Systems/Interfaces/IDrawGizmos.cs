namespace DesertImage.ECS
{
    public interface IDrawGizmos : ISystem
    {
        void DrawGizmos(in World world);
    }
}