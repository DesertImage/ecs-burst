namespace DesertImage.ECS
{
    public struct TransformStuffFeature : IFeature
    {
        public void Link(World world)
        {
            world.Add<TransformToEntitySystem>(ExecutionType.MainThread);
            world.Add<EntityToTransformSystem>(ExecutionType.MainThread);
        }
    }
}