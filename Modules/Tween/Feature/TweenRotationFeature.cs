using DesertImage.ECS;

namespace Game.Tween
{
    public struct TweenRotationFeature : IFeature
    {
        public void Link(World world)
        {
            world.Add<TweenRotationCancelSystem>();
            world.Add<TweenRotationTimeSystem>();
            world.Add<TweenRotationSystem>();

            world.Add<RemoveComponentSystem<TweenRotationCancel>>();
        }
    }
}