using DesertImage.ECS;
using Group;

namespace DesertImage
{
    public struct GroupAddedEvent
    {
        public IMatcher Matcher;
        public EntitiesGroup Value;
    }
}