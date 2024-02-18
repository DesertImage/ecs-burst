using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public unsafe struct ExecuteSystemWrapper
    {
        public Matcher Matcher;
        [NativeDisableUnsafePtrRestriction] public void* Value;
        [NativeDisableUnsafePtrRestriction] public void* MethodPtr;
    }
}