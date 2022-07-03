using NUnit.Framework;
using UnityEngine;

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

        [Test]
        public void ComponentPoolingOnEntityMono()
        {
            var testCore = new TestCore();

            var world = testCore.Get<World>();

            var gameObject = new GameObject("test");
            gameObject.AddComponent<EntityMono>();

            var spawnService = new SpawnService();
            spawnService.Register(0, gameObject);

            testCore.Add(spawnService);

            for (var i = 0; i < 100; i++)
            {
                var entity = spawnService.Spawn(0).GetComponent<IEntity>();

                var wrapper = new TestEntityWrapper();
                wrapper.Link(entity);

                entity.AddTestComponent();

                Assert.AreEqual(5, entity.GetTestValueComponentValue());

                entity.ReturnToPool();
            }
        }

        [Test]
        public void ComplexPoolComponents()
        {
            var testCore = new TestCore();

            var world = testCore.Get<World>();

            // for (var j = 0; j < 10; j++)
            // {
                var entity = world.GetNewEntity();
                entity.AddTestValueComponent(5);
                
                for (var i = 0; i < 2; i++)
                {
                    entity.SetTestValueComponentValue(entity.GetTestValueComponentValue() + 1);
                }
            // }
        }
    }
}