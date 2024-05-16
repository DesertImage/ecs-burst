using DesertImage.ECS;

namespace Game.Tween
{
    public struct TweenFeature : IFeature
    {
        public void Link(World world)
        {
            new TweenPositionFeature().Link(world);
            new TweenLocalPositionFeature().Link(world);

            new TweenRotationFeature().Link(world);
            new TweenLocalRotationFeature().Link(world);

            new TweenScaleFeature().Link(world);

            world.AddRemoveComponentSystem<TweenCancelAll>();
        }
    }
}