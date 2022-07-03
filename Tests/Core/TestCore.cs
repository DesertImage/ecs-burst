namespace DesertImage.ECS.Tests
{
    public class TestCore : Core
    {
        public TestCore()
        {
            Add<ManagerUpdate>();

            var world = Add(new World());

            Add(new SystemsManager(world));
        }
    }
}