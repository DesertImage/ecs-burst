using System;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public unsafe struct ExecuteSystemWrapper : IDisposable
    {
        [NativeDisableUnsafePtrRestriction] public void* Value;
        [NativeDisableUnsafePtrRestriction] public void* MethodPtr;
        
        public void Dispose()
        {
            MemoryUtility.Free(Value);
            MethodPtr = null;
        }
    }
}