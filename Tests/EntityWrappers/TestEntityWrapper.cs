namespace DesertImage.ECS.Tests
{
    public class TestEntityWrapper : IComponentWrapper
    {
        public void Link(IComponentHolder componentHolder)
        {
            componentHolder.AddTestValueComponent(5);
        }
    }
}