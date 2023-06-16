namespace DesertImage.ECS
{
    public interface IMatchSystem : ISystem
    {
        Matcher Matcher { get; }
    }
}