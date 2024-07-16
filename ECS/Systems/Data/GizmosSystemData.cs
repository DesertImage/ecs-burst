using System;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public unsafe struct GizmosSystemData : IDisposable, IEquatable<GizmosSystemData>
    {
        public uint Id;
        [NativeDisableUnsafePtrRestriction] public GizmosSystemWrapper* Wrapper;

        public void Dispose()
        {
            Wrapper->Dispose();
            MemoryUtility.Free(Wrapper);
        }

        public bool Equals(GizmosSystemData other) => other.Id == Id;
    }
}