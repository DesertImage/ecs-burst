namespace DesertImage.ECS
{
    public struct TransformStuffFeature : IFeature
    {
        public void Link(World world)
        {
            world.Add<TransformToEntitySystem>(ExecutionOrder.EarlyMainThread);
            world.Add<EntityToTransformSystem>(ExecutionOrder.EarlyMainThread);
        }
    }
}