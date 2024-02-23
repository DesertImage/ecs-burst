using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public unsafe struct ExecuteSystemWrapper
    {
        public ushort MatcherId;
        [NativeDisableUnsafePtrRestriction] public void* Value;
        [NativeDisableUnsafePtrRestriction] public void* MethodPtr;
    }
}