using DesertImage.ECS;

namespace Game.Tween
{
    public struct TweenLocalRotationFeature : IFeature
    {
        public void Link(World world)
        {
            world.Add<TweenLocalRotationCancelSystem>();
            world.Add<TweenLocalRotationTimeSystem>();
            world.Add<TweenLocalRotationSystem>();

            world.Add<RemoveComponentSystem<TweenLocalRotationCancel>>();
        }
    }
}