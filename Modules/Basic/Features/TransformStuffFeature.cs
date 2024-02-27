namespace DesertImage.ECS
{
    public struct TransformStuffFeature : IFeature
    {
        public void Link(World world)
        {
            world.Add<TransformToEntitySystem>(ExecutionType.EarlyMainThread);
            world.Add<EntityToTransformSystem>(ExecutionType.LateMainThread);
        }
    }
}