using NUnit.Framework;

namespace DesertImage.ECS.Tests
{
    public class Systems
    {
        [Test]
        public void CheckSystemExecute()
        {
            var world = Worlds.Create();

            var entity = world.GetNewEntity();

            entity.Replace(new TestValueComponent { Value = 2 });

            world.Add<TestValueSystem>();

            world.Tick(.1f);

            var firstResult = entity.Get<TestValueComponent>().Value;
            var secondResult = entity.Get<TestValueComponent>().Value;

            entity.Replace<TestComponent>();

            world.Tick(.1f);

            entity.Remove<TestComponent>();

            world.Tick(.1f);

            var thirdResult = entity.Get<TestValueComponent>().Value;

            entity.Remove<TestValueComponent>();

            world.Tick(.1f);

            entity.Replace(new TestValueComponent { Value = 1 });

            world.Tick(.1f);

            var fourthResult = entity.Get<TestValueComponent>().Value;

            world.Dispose();

            Assert.AreEqual(3, firstResult);
            Assert.AreEqual(3, secondResult);
            Assert.AreEqual(4, thirdResult);
            Assert.AreEqual(2, fourthResult);
        }
    }
}