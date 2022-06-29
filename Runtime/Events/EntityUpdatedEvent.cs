using DesertImage.ECS;
using Group;

namespace DesertImage
{
    public struct EntityUpdatedEvent
    {
        public EntitiesGroup Group;
        public IComponent Component;
        public IEntity Value;
    }
}