namespace DesertImage.ECS
{
    public struct EntityUpdatedEvent
    {
        public EntitiesGroup Group;
        public IComponent Component;
        public IEntity Value;
    }
}