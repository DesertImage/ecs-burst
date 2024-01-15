using System.Collections.Generic;

namespace DesertImage.ECS
{
    public class ComponentsSharedStorage<T> : ComponentsStorageBase
    {
        public T Data;
        public readonly HashSet<int> Entities;

        public ComponentsSharedStorage(HashSet<int> entities)
        {
            Entities = entities;
        }
    }
}