using NUnit.Framework;

namespace DesertImage.ECS.Tests
{
    public class Tests
    {
        [Test]
        public void GroupDisposedEntityRemoved()
        {
            var testCore = new TestCore();

            var world = testCore.Get<World>();

            var entity = world.GetNewEntity();

            entity.AddTestComponent();
            
            var group = world.GetGroup(Match.AllOf(0));
            
            Assert.AreEqual(1, group.Entities.Count);
            
            entity.Dispose();
            
            Assert.AreEqual(0, group.Entities.Count);
        }
    }
}