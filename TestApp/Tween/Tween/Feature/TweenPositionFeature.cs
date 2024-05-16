using DesertImage.ECS;

namespace Game.Tween
{
    public struct TweenPositionFeature : IFeature
    {
        public void Link(World world)
        {
            world.Add<TweenPositionCancelSystem>();
            world.Add<TweenPositionTimeSystem>();
            world.Add<TweenPositionSystem>();

            world.AddRemoveComponentSystem<TweenPositionCancel>();
        }
    }
}