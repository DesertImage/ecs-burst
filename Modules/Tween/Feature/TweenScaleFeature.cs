using DesertImage.ECS;

namespace Game.Tween
{
    public struct TweenScaleFeature : IFeature
    {
        public void Link(World world)
        {
            world.Add<TweenScaleCancelSystem>();
            world.Add<TweenScaleTimeSystem>();
            world.Add<TweenScaleSystem>();

            world.Add<RemoveComponentSystem<TweenScaleCancel>>();
        }
    }
}