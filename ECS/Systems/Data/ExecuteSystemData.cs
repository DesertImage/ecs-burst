using System;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public unsafe struct ExecuteSystemData : IDisposable
    {
        public uint Id;
        [NativeDisableUnsafePtrRestriction] public ExecuteSystemWrapper* Wrapper;
        
        public void Dispose()
        {
            Wrapper->Dispose();
            MemoryUtility.Free(Wrapper);
        }
    }
}