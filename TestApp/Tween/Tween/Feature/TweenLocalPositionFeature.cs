using DesertImage.ECS;

namespace Game.Tween
{
    public struct TweenLocalPositionFeature : IFeature
    {
        public void Link(World world)
        {
            world.Add<TweenLocalPositionCancelSystem>();
            world.Add<TweenLocalPositionTimeSystem>();
            world.Add<TweenLocalPositionSystem>();

            world.Add<RemoveComponentSystem<TweenLocalPositionCancel>>();
        }
    }
}