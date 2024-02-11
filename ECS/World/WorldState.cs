using System;
using DesertImage.Collections;
using DesertImage.ECS;

namespace DesertImage.ECS
{
    public unsafe struct WorldState : IDisposable
    {
        public UnsafeArray<Entity> Entities;
        public UnsafeSparseSet<UnsafeSparseSet<int>> EntityComponents;

        //TODO: move byte* data here?
        public ComponentStorage Components;
        // public byte* SharedComponents;
        // public byte* StaticComponents;

        // public ComponentsStorageBase[] Storages;
        // public readonly ComponentsStorageBase[] SharedStorages;
        // public readonly ComponentsStorageBase[] StaticStorages;

        public WorldState
        (
            UnsafeArray<Entity> entities,
            UnsafeSparseSet<UnsafeSparseSet<int>> entityComponents,
            int componentsCapacity,
            int entitiesCapacity
        ) : this()
        {
            Entities = entities;
            EntityComponents = entityComponents;
            Components = new ComponentStorage(componentsCapacity * entitiesCapacity, entitiesCapacity);
        }

        public void Dispose()
        {
            foreach (var entityComponent in EntityComponents)
            {
                entityComponent.Dispose();
            }

            Components.Dispose();
            // UnsafeUtility.Free(SharedComponents, Allocator.Persistent);
            // UnsafeUtility.Free(StaticComponents, Allocator.Persistent);

            Entities.Dispose();
            EntityComponents.Dispose();
        }
    }
}