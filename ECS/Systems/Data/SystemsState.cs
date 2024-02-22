using System;
using DesertImage.Collections;
using Unity.Jobs;

namespace DesertImage.ECS
{
    public struct SystemsState : IDisposable
    {
        public UnsafeList<ExecuteSystemData> EarlyMainThreadSystems;
        public UnsafeList<ExecuteSystemData> LateMainThreadSystems;
        public UnsafeList<ExecuteSystemData> MultiThreadSystems;
        public UnsafeHashSet<uint> SystemsHash;

        public float DeltaTime;

        public JobHandle Handle;

        public void Dispose()
        {
            EarlyMainThreadSystems.Dispose();
            LateMainThreadSystems.Dispose();
            MultiThreadSystems.Dispose();

            SystemsHash.Dispose();
        }
    }
}