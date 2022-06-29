using DesertImage.ECS;
using Group;

namespace DesertImage
{
    public struct EntityRemovedEvent
    {
        public EntitiesGroup Group;
        public IEntity Value;
    }
}