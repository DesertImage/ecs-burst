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
        public UnsafeDictionary<ushort, EntitiesGroup> Groups;
        public UnsafeDictionary<ushort, Matcher> Matchers;
        public UnsafeDictionary<ushort, ushort> MatcherToGroup;
        public UnsafeDictionary<ushort, ushort> GroupToMatcher;
        public UnsafeDictionary<uint, UnsafeList<ushort>> EntityToGroups;
        public UnsafeDictionary<uint, UnsafeList<ushort>> ComponentToGroups;

        public WorldState(int componentsCapacity, int entitiesCapacity)
        {
            EntityIdCounter = 0;
            AliveEntities = new UnsafeSparseSet<uint>(entitiesCapacity);
            EntitiesPool = new UnsafeQueue<uint>(100, Allocator.Persistent);

            Components = new ComponentStorage(componentsCapacity, entitiesCapacity);
            StaticComponents = new UnsafeDictionary<uint, IntPtr>(20, Allocator.Persistent);

            GroupIdCounter = 0;
            Groups = new UnsafeDictionary<ushort, EntitiesGroup>(20, Allocator.Persistent);

            Matchers = new UnsafeDictionary<ushort, Matcher>(20, Allocator.Persistent);

            MatcherToGroup = new UnsafeDictionary<ushort, ushort>(20, Allocator.Persistent);
            GroupToMatcher = new UnsafeDictionary<ushort, ushort>(20, Allocator.Persistent);

            EntityToGroups = new UnsafeDictionary<uint, UnsafeList<ushort>>(entitiesCapacity, Allocator.Persistent);
            ComponentToGroups = new UnsafeDictionary<uint, UnsafeList<ushort>>(20, Allocator.Persistent);
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