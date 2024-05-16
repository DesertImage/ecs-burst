using DesertImage.ECS;

namespace Game.DragAndDrop
{
    public struct DragAndDropFeature : IFeature
    {
        public void Link(World world)
        {
            world.Add<DragMousePositionSystem>();
            world.Add<DragSystem>();
            world.Add<DragLocalPositionSystem>();
            world.Add<DropSystem>();

            world.AddRemoveComponentSystem<DragStartTag>();
            world.AddRemoveComponentSystem<DropTag>();
        }
    }
}