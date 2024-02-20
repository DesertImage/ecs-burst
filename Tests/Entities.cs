using DesertImage.Collections;
using NUnit.Framework;
using Unity.Collections;

namespace DesertImage.ECS.Tests
{
    public class Entities
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
        public void CreateThousands()
        {
            const int entitiesCount = 100000;

            var world = Worlds.Create();

            var entities = new UnsafeArray<Entity>(entitiesCount, Allocator.Persistent);

            for (var i = 0; i < entitiesCount; i++)
            {
                var newEntity = world.GetNewEntity();
                entities[i] = newEntity;

                if (i % 2 == 0)
                {
                    newEntity.Replace<TestComponent>();
                }
            }

            entities.Dispose();
            world.Dispose();
        }
    }
}