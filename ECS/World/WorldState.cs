using System;
using DesertImage.Collections;
using Unity.Collections;

namespace DesertImage.ECS
{
    public struct WorldState : IDisposable
    {
        public uint EntityIdCounter;
        public UnsafeSparseSet<uint> AliveEntities;
        public UnsafeQueue<uint> EntitiesPool;

        public ComponentStorage Components;
        public UnsafeDictionary<uint, IntPtr> StaticComponents;

        public uint GroupIdCounter;
        public UnsafeDictionary<uint, EntitiesGroup> Groups;
        public UnsafeDictionary<uint, Matcher> Matchers;
        public UnsafeDictionary<uint, uint> MatcherToGroup;
        public UnsafeDictionary<uint, uint> GroupToMatcher;
        public UnsafeDictionary<uint, UnsafeList<uint>> EntityToGroups;
        public UnsafeDictionary<uint, UnsafeList<uint>> ComponentToGroups;

        public WorldState(int componentsCapacity, int entitiesCapacity)
        {
            EntityIdCounter = 0;
            AliveEntities = new UnsafeSparseSet<uint>(entitiesCapacity);
            EntitiesPool = new UnsafeQueue<uint>(100, Allocator.Persistent);

            Components = new ComponentStorage(componentsCapacity, entitiesCapacity);
            StaticComponents = new UnsafeDictionary<uint, IntPtr>(20, Allocator.Persistent);

            GroupIdCounter = 0;
            Groups = new UnsafeDictionary<uint, EntitiesGroup>(20, Allocator.Persistent);

            Matchers = new UnsafeDictionary<uint, Matcher>(20, Allocator.Persistent);

            MatcherToGroup = new UnsafeDictionary<uint, uint>(20, Allocator.Persistent);
            GroupToMatcher = new UnsafeDictionary<uint, uint>(20, Allocator.Persistent);

            EntityToGroups = new UnsafeDictionary<uint, UnsafeList<uint>>(20, Allocator.Persistent);
            ComponentToGroups = new UnsafeDictionary<uint, UnsafeList<uint>>(20, Allocator.Persistent);
        }

        public unsafe void Dispose()
        {
            EntityIdCounter = 0;
            AliveEntities.Dispose();
            EntitiesPool.Dispose();

            GroupIdCounter = 0;
            MatcherToGroup.Dispose();
            GroupToMatcher.Dispose();

            foreach (var pair in Groups)
            {
                pair.Value.Dispose();
            }

            Groups.Dispose();

            foreach (var pair in EntityToGroups)
            {
                pair.Value.Dispose();
            }
            EntityToGroups.Dispose();

            Components.Dispose();

            foreach (var pair in StaticComponents)
            {
                MemoryUtility.Free((void*)pair.Value);
            }
            StaticComponents.Dispose();
        }
    }
}