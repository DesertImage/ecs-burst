using System;
using DesertImage.Collections;
using Unity.Collections;

namespace DesertImage.ECS
{
    public struct WorldState : IDisposable
    {
        public uint EntityIdCounter;
        public UnsafeUintSparseSet<uint> AliveEntities;
        public UnsafeQueue<uint> EntitiesPool;

        public ComponentStorage Components;
        public UnsafeUintSparseSet<IntPtr> StaticComponents;

        public ushort GroupIdCounter;
        public UnsafeUshortSparseSet<EntitiesGroup> Groups;
        public UnsafeUintSparseSet<UnsafeList<ushort>> EntityToGroups;
        public UnsafeUintSparseSet<UnsafeList<ushort>> ComponentToGroups;

        public WorldState(int componentsCapacity, int entitiesCapacity)
        {
            const int groupsCapacity = 20;

            EntityIdCounter = 0;
            AliveEntities = new UnsafeUintSparseSet<uint>(entitiesCapacity);
            EntitiesPool = new UnsafeQueue<uint>(100, Allocator.Persistent);

            Components = new ComponentStorage(componentsCapacity, entitiesCapacity);
            StaticComponents = new UnsafeUintSparseSet<IntPtr>(20);

            GroupIdCounter = 0;
            Groups = new UnsafeUshortSparseSet<EntitiesGroup>(groupsCapacity);

            EntityToGroups = new UnsafeUintSparseSet<UnsafeList<ushort>>(entitiesCapacity);
            ComponentToGroups = new UnsafeUintSparseSet<UnsafeList<ushort>>(componentsCapacity);
        }

        public unsafe void Dispose()
        {
            EntityIdCounter = 0;
            AliveEntities.Dispose();
            EntitiesPool.Dispose();

            GroupIdCounter = 0;

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

            foreach (var value in ComponentToGroups)
            {
                value.Dispose();
            }

            ComponentToGroups.Dispose();
            
            Components.Dispose();

            for (var i = 0; i < StaticComponents.Count; i++)
            {
                MemoryUtility.Free((void*)StaticComponents._dense[i]);
            }

            StaticComponents.Dispose();
        }
    }
}