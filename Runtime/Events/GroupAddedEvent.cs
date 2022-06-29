namespace DesertImage.ECS
{
    public struct GroupAddedEvent
    {
        public IMatcher Matcher;
        public EntitiesGroup Value;
    }
}