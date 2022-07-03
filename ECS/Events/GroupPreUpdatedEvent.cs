namespace DesertImage.ECS
{
    public struct GroupPreUpdatedEvent
    {
        public IMatcher Matcher;
        public EntitiesGroup Value;
    }
}