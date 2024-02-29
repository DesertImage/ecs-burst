using DesertImage.ECS;

namespace Game.Tween
{
    public struct TweenPositionFeature : IFeature
    {
        public void Link(World world)
        {
            world.Add<TweenPositionCancelSystem>(ExecutionType.LateMainThread);
            world.Add<TweenPositionTimeSystem>(ExecutionType.LateMainThread);
            world.Add<TweenPositionSystem>(ExecutionType.LateMainThread);

            world.Add<RemoveComponentSystem<TweenPositionCancel>>();
        }
    }
}