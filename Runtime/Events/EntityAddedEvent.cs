using DesertImage.ECS;
using Group;

namespace DesertImage
{
    public struct EntityAddedEvent
    {
        public EntitiesGroup Group;
        public IEntity Value;
    }
}