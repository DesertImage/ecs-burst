using NUnit.Framework;

namespace DesertImage.ECS
{
    public class EntitiesTests
    {
        [Test]
        public void Create()
        {
            var world = Worlds.Create();

            var entity = world.GetNewEntity();
            entity.Destroy();

            world.Dispose();
        }

        [Test]
        public void Create_2_000()
        {
            const int entitiesCount = 2_000;

            var world = Worlds.Create();

            for (var i = 0; i < entitiesCount; i++)
            {
                world.GetNewEntity();
            }

            world.Dispose();
        }
        
        [Test]
        public void Create_10_000()
        {
            const int entitiesCount = 10_000;

            var world = Worlds.Create();

            for (var i = 0; i < entitiesCount; i++)
            {
                world.GetNewEntity();
            }

            world.Dispose();
        }

        [Test]
        public void Create_100_000()
        {
            const int entitiesCount = 100_000;

            var world = Worlds.Create();

            for (var i = 0; i < entitiesCount; i++)
            {
                world.GetNewEntity();
            }

            world.Dispose();
        }

        [Test]
        public void Create_1_000_000()
        {
            const int entitiesCount = 1_000_000;

            var world = Worlds.Create();

            for (var i = 0; i < entitiesCount; i++)
            {
                world.GetNewEntity();
            }

            world.Dispose();
        }
    }
}