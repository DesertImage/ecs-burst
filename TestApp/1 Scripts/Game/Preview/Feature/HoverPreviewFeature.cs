using DesertImage.ECS;

namespace Game
{
    public struct HoverPreviewFeature : IFeature
    {
        public void Link(World world)
        {
            world.Add<PreviewHoverSystem>();
            world.Add<PreviewUnhoverSystem>();
            
            world.AddRemoveComponentSystem<PreviewTag>();
            world.AddRemoveComponentSystem<UnPreviewTag>();
        }
    }
}