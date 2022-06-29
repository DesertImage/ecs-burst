namespace DesertImage.ECS
{
    public struct ComponentRemovedEvent
    {
        public IComponentHolder Holder;

        public IComponent Value;
    }
}