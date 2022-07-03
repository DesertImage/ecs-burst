namespace DesertImage.ECS
{
    public struct EntityPreUpdatedEvent
    {
        public EntitiesGroup Group;
        public IEntity Value;
        public IComponent Previous;
        public IComponent Future;
    }
}