using DesertImage.ECS;

namespace Game.Physics
{
    public struct FakePhysicalObjectFeature : IFeature
    {
        public void Link(World world)
        {
            world.Add<FakePhysicalDecorSystem>(ExecutionOrder.Physics);
            world.Add<PositionDeltaSystem>(ExecutionOrder.Physics);
        }
    }
}