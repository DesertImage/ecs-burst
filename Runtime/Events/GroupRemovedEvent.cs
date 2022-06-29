namespace DesertImage.ECS
{
    public struct GroupRemovedEvent
    {
        public IMatcher Matcher;
        public EntitiesGroup Value;
    }
}