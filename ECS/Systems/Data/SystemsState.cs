using System;
using DesertImage.Collections;
using Unity.Collections;
using Unity.Jobs;

namespace DesertImage.ECS
{
    public struct SystemsState : IDisposable
    {
        public UnsafeList<ExecuteSystemData> EarlyMainThreadSystems;
        public UnsafeList<ExecuteSystemData> LateMainThreadSystems;
        public UnsafeList<ExecuteSystemData> MultiThreadSystems;
        public UnsafeUintSparseSet<uint> SystemsHash;

        public JobHandle Handle;

        public SystemsState(int capacity)
        {
            EarlyMainThreadSystems = new UnsafeList<ExecuteSystemData>(20, Allocator.Persistent);
            MultiThreadSystems = new UnsafeList<ExecuteSystemData>(20, Allocator.Persistent);
            LateMainThreadSystems = new UnsafeList<ExecuteSystemData>(20, Allocator.Persistent);
            SystemsHash = new UnsafeUintSparseSet<uint>(20);

            Handle = default;
        }

        public void Dispose()
        {
            EarlyMainThreadSystems.Dispose();
            LateMainThreadSystems.Dispose();
            MultiThreadSystems.Dispose();

            SystemsHash.Dispose();
        }
    }
}