using NUnit.Framework;

namespace DesertImage.ECS.Tests
{
    public class StressTests
    {
        [Test]
        public void Entities()
        {
            const int iterations = 5;
            const int entitiesCount = 100_000;

            for (var i = 0; i < iterations; i++)
            {
                var world = Worlds.Create();

                for (var j = 0; j < entitiesCount; j++)
                {
                    world.GetNewEntity();
                }

                world.Dispose();
            }
        }

        [Test]
        public void Components()
        {
            const int iterations = 5;
            const int entitiesCount = 100_000;

            for (var i = 0; i < iterations; i++)
            {
                var world = Worlds.Create();

                for (var j = 0; j < entitiesCount; j++)
                {
                    var entity = world.GetNewEntity();
                    entity.Replace<TestComponent>();
                }

                world.Dispose();
            }
        }

        [Test]
        public void Groups()
        {
            const int iterations = 10;
            const int groupsCount = 100;
            const int componentsCount = 50;

            for (var i = 0; i < iterations; i++)
            {
                var world = Worlds.Create();

                for (var j = 0; j < groupsCount; j++)
                {
                    var group = world.GetNewGroup();

                    for (var k = 1; k < componentsCount + 1; k++)
                    {
                        group.With((uint)k, 4);
                    }
                }

                world.Dispose();
            }
        }
    }
}