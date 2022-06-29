using DesertImage.ECS;
using Group;

namespace DesertImage.ECS
{
    public struct EntityAddedEvent
    {
        public EntitiesGroup Group;
        public IEntity Value;
    }
}