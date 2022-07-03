namespace DesertImage.ECS
{
    public struct ComponentUpdatedEvent
    {
        public IComponentHolder Holder;

        public IComponent Value;
    }
}