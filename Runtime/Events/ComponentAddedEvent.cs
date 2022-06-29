namespace DesertImage.ECS
{
    public struct ComponentAddedEvent
    {
        public IComponentHolder Holder;
        public IComponent Value;
    }
}