namespace DesertImage.ECS
{
    public interface IExecuteFixedUpdateSystem  : IMatchSystem
    {
        void Execute(IEntity entity);
    }
}