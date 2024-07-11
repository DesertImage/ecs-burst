using System;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public unsafe struct DestroySystemData : IDisposable, IEquatable<DestroySystemData>
    {
        public uint Id;
        [NativeDisableUnsafePtrRestriction] public DestroySystemWrapper* Wrapper;

        public void Dispose()
        {
            Wrapper->Dispose();
            MemoryUtility.Free(Wrapper);
        }

        public bool Equals(DestroySystemData other) => other.Id == Id;
    }
}