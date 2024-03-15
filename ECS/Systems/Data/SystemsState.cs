using System;
using DesertImage.Collections;
using Unity.Collections;

namespace DesertImage.ECS
{
    public struct SystemsState : IDisposable
    {
        public UnsafeList<ExecuteSystemData> EarlyMainThreadSystems;
        public UnsafeList<ExecuteSystemData> MultiThreadSystems;
        public UnsafeList<ExecuteSystemData> LateMainThreadSystems;
        public UnsafeList<ExecuteSystemData> RemoveTagsSystems;
        public UnsafeUintSparseSet<uint> SystemsHash;

        public SystemsContext Context;

        public SystemsState(int capacity)
        {
            EarlyMainThreadSystems = new UnsafeList<ExecuteSystemData>(capacity, Allocator.Persistent);
            MultiThreadSystems = new UnsafeList<ExecuteSystemData>(capacity, Allocator.Persistent);
            LateMainThreadSystems = new UnsafeList<ExecuteSystemData>(capacity, Allocator.Persistent);
            RemoveTagsSystems = new UnsafeList<ExecuteSystemData>(capacity, Allocator.Persistent);
            SystemsHash = new UnsafeUintSparseSet<uint>(capacity);
            Context = new SystemsContext();
        }

        private static void DisposeSystems(UnsafeList<ExecuteSystemData> values)
        {
            for (var i = 0; i < values.Count; i++)
            {
                values[i].Dispose();
            }

            values.Dispose();
        }

        public void Dispose()
        {
            DisposeSystems(EarlyMainThreadSystems);
            DisposeSystems(MultiThreadSystems);
            DisposeSystems(LateMainThreadSystems);
            DisposeSystems(RemoveTagsSystems);

            SystemsHash.Dispose();
        }
    }
}