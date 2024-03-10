using DesertImage.ECS;

namespace Game
{
    public struct OriginsFeature : IFeature
    {
        public void Link(World world)
        {
            world.Add<ParentToOriginSystem>(ExecutionType.MainThread);
            world.Add<LocalPositionSystem>();
            world.Add<LocalRotationSystem>();
        }
    }
}