using System;
using Unity.Burst;

namespace DesertImage.ECS
{
    public struct SystemsState : IDisposable
    {
        public UnsafeList<ExecuteSystemData> ExecuteSystems;
        public UnsafeList<FunctionPointer<SystemsTools.Destroy>> DestroySystems;
        public UnsafeHashSet<int> SystemsHash;

        public void Dispose()
        {
            ExecuteSystems.Dispose();
            DestroySystems.Dispose();
            SystemsHash.Dispose();
        }
    }
}