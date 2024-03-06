namespace DesertImage.ECS
{
    public unsafe interface IExecuteSystem : ISystem
    {
        void Execute(ref SystemsContext context);
    }
}