using DesertImage.ECS;

namespace Game.Vehicle
{
    public struct GearboxFeature : IFeature
    {
        public void Link(World world)
        {
            world.Add<GearboxInputSystem>(ExecutionOrder.EarlyMainThread);
            
            // world.Add<GearboxAutomaticSystem>();
            world.Add<GearboxSystem>();

            world.AddRemoveComponentSystem<GearUpTag>();
            world.AddRemoveComponentSystem<GearDownTag>();
            world.AddRemoveComponentSystem<ComponentUpdated<Gearbox>>();
        }
    }
}