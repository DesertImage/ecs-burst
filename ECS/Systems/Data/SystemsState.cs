using System;
using DesertImage.Collections;
using Unity.Jobs;

namespace DesertImage.ECS
{
    public struct SystemsState : IDisposable
    {
        public UnsafeList<ExecuteSystemData> ExecuteSystems;
        public UnsafeHashSet<uint> SystemsHash;

        public float DeltaTime;

        public JobHandle Handle;

        public void Dispose()
        {
            ExecuteSystems.Dispose();
            SystemsHash.Dispose();
        }
    }
}