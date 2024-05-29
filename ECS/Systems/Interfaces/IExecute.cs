namespace DesertImage.ECS
{
    public unsafe interface IExecute : ISystem
    {
        void Execute(ref SystemsContext context);
    }
}