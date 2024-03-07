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

            var systemsContext = new SystemsContext();
            Context = MemoryUtility.AllocateInstance(in systemsContext);
        }

        public void Dispose()
        {
            for (var i = 0; i < EarlyMainThreadSystems.Count; i++)
            {
                EarlyMainThreadSystems[i].Dispose();
            }
            
            EarlyMainThreadSystems.Dispose();
            
            for (var i = 0; i < MultiThreadSystems.Count; i++)
            {
                MultiThreadSystems[i].Dispose();
            }
            
            MultiThreadSystems.Dispose();

            SystemsHash.Dispose();

            MemoryUtility.Free(Context);
        }
    }
}