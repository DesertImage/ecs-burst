using DesertImage.ECS;
using Group;

namespace DesertImage
{
    public struct GroupRemovedEvent
    {
        public IMatcher Matcher;
        public EntitiesGroup Value;
    }
}