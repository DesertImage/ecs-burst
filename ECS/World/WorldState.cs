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

        public ushort GroupIdCounter;
        public UnsafeUshortSparseSet<EntitiesGroup> Groups;
        public UnsafeUshortSparseSet<Matcher> Matchers;
        public UnsafeDictionary<ushort, ushort> MatcherToGroup;
        public UnsafeUshortSparseSet<ushort> GroupToMatcher;
        public UnsafeUintSparseSet<UnsafeList<ushort>> EntityToGroups;
        public UnsafeUintSparseSet<UnsafeList<ushort>> ComponentToGroups;

        public WorldState(int componentsCapacity, int entitiesCapacity)
        {
            const int groupsCapacity = 20;

            EntityIdCounter = 0;
            AliveEntities = new UnsafeSparseSet<uint>(entitiesCapacity);
            EntitiesPool = new UnsafeQueue<uint>(100, Allocator.Persistent);

            Components = new ComponentStorage(componentsCapacity, entitiesCapacity);
            StaticComponents = new UnsafeDictionary<uint, IntPtr>(20, Allocator.Persistent);

            GroupIdCounter = 0;
            Groups = new UnsafeUshortSparseSet<EntitiesGroup>(groupsCapacity);

            Matchers = new UnsafeUshortSparseSet<Matcher>(groupsCapacity);

            MatcherToGroup = new UnsafeDictionary<ushort, ushort>(groupsCapacity, Allocator.Persistent);
            GroupToMatcher = new UnsafeUshortSparseSet<ushort>(groupsCapacity);

            EntityToGroups = new UnsafeUintSparseSet<UnsafeList<ushort>>(entitiesCapacity);
            ComponentToGroups = new UnsafeUintSparseSet<UnsafeList<ushort>>(componentsCapacity);
        }

        public unsafe void Dispose()
        {
            EntityIdCounter = 0;
            AliveEntities.Dispose();
            EntitiesPool.Dispose();

            GroupIdCounter = 0;
            MatcherToGroup.Dispose();
            GroupToMatcher.Dispose();

            foreach (var value in Groups)
            {
                value.Dispose();
            }

            Groups.Dispose();

            foreach (var value in EntityToGroups)
            {
                value.Dispose();
            }

            EntityToGroups.Dispose();

            Components.Dispose();

            foreach (var pair in StaticComponents)
            {
                if (pair.Key == 0) continue;
                MemoryUtility.Free((void*)pair.Value);
            }

            StaticComponents.Dispose();
        }
    }
}