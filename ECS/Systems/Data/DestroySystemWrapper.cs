using System;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public unsafe struct DestroySystemWrapper : IDisposable
    {
        [NativeDisableUnsafePtrRestriction] public void* Value;
        [NativeDisableUnsafePtrRestriction] public void* MethodPtr;

        public byte DoNotFree;
        
        public void Dispose()
        {
            if (DoNotFree != 1)
            {
                MemoryUtility.Free(Value);
            }

            Value = null;
            MethodPtr = null;
        }
    }
}