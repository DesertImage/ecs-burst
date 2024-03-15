using System;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public unsafe struct ExecuteSystemData : IDisposable, IEquatable<ExecuteSystemData>
    {
        public uint Id;
        [NativeDisableUnsafePtrRestriction] public ExecuteSystemWrapper* Wrapper;

        public void Dispose()
        {
            Wrapper->Dispose();
            MemoryUtility.Free(Wrapper);
        }

        public bool Equals(ExecuteSystemData other) => other.Id == Id;
    }
}