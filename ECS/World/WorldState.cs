using System;
using System.Collections.Generic;

namespace DesertImage.ECS
{
    [Serializable]
    public struct WorldState
    {
        public readonly Dictionary<int, Entity> Entities;
        public readonly Dictionary<int, SortedSetPoolable<int>> EntityComponents;
        public ComponentsStorageBase[] Storages;
        public readonly ComponentsStorageBase[] SharedStorages;
        public readonly ComponentsStorageBase[] StaticStorages;

        public WorldState
        (
            Dictionary<int, Entity> entities,
            Dictionary<int, SortedSetPoolable<int>> entityComponents,
            ComponentsStorageBase[] storages,
            ComponentsStorageBase[] sharedStorages,
            ComponentsStorageBase[] staticStorages
        )
        {
            Entities = entities;
            EntityComponents = entityComponents;
            Storages = storages;
            SharedStorages = sharedStorages;
            StaticStorages = staticStorages;
        }
    }
}