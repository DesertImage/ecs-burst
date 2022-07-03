namespace DesertImage.ECS
{
    public struct ComponentPreUpdatedEvent
    {
        public IComponentHolder Holder;

        public IComponent PreviousValue;
        public IComponent FutureValue;
    }
}