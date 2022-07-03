namespace DesertImage.ECS
{
    public struct GroupUpdatedEvent
    {
        public IMatcher Matcher;
        public EntitiesGroup Value;
    }
}