using System.Collections.Generic;
using DesertImage.ECS;

namespace Entities
{
    public interface EntityComparer : IEqualityComparer<Entity>
    {
    }

    public class EntityEqualityComparer : EntityComparer
    {
        public bool Equals(Entity x, Entity y) => x.Id == y.Id;

        public int GetHashCode(Entity obj) => obj.Id;
    }
}