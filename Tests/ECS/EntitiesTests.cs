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
        public void Create_1000_000()
        {
            const int entitiesCount = 1_000_000;

            var world = Worlds.Create();

            for (var i = 0; i < entitiesCount; i++)
            {
                world.GetNewEntity();
            }

            world.Dispose();
        }

        [Test]
        public void EntityDestroyLastComponentRemoved()
        {
            var world = Worlds.Create();
            var entity = world.GetNewEntity();

            entity.Replace<TestComponent>();

            var firstCheck = entity.IsAlive();

            entity.Replace<TestTag>();

            var secondCheck = entity.IsAlive();

            entity.Remove<TestComponent>();

            var thirdCheck = entity.IsAlive();

            entity.Replace<TestValueComponent>();

            var fourthCheck = entity.IsAlive();
            
            entity.Remove<TestTag>();

            var fifthCheck = entity.IsAlive();

            entity.Remove<TestValueComponent>();

            var sixthCheck = entity.IsAlive();

            world.Dispose();

            Assert.IsTrue(firstCheck);
            Assert.IsTrue(secondCheck);
            Assert.IsTrue(thirdCheck);
            Assert.IsTrue(fourthCheck);
            Assert.IsTrue(fifthCheck);
            Assert.IsFalse(sixthCheck);
        }
    }
}