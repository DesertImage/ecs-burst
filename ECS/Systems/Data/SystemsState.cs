using System;
using DesertImage.Collections;
using Unity.Collections;

namespace DesertImage.ECS
{
    public unsafe struct SystemsState : IDisposable
    {
        public UnsafeList<ExecuteSystemData> EarlyMainThreadSystems;
        public UnsafeList<ExecuteSystemData> MultiThreadSystems;
        public UnsafeUintSparseSet<uint> SystemsHash;

        public SystemsContext* Context;

        public SystemsState(int capacity)
        {
            EarlyMainThreadSystems = new UnsafeList<ExecuteSystemData>(capacity, Allocator.Persistent);
            MultiThreadSystems = new UnsafeList<ExecuteSystemData>(capacity, Allocator.Persistent);
            SystemsHash = new UnsafeUintSparseSet<uint>(capacity);

            Context = MemoryUtility.Allocate(new SystemsContext());
        }

        public void Dispose()
        {
            EarlyMainThreadSystems.Dispose();
            MultiThreadSystems.Dispose();

            SystemsHash.Dispose();

            MemoryUtility.Free(Context);
        }
    }
}