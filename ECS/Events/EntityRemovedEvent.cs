namespace DesertImage.ECS
{
    public struct EntityRemovedEvent
    {
        public EntitiesGroup Group;
        public IEntity Value;
    }
}