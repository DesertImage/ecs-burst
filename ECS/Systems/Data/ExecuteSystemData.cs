using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public unsafe struct ExecuteSystemData
    {
        public uint Id;
        [NativeDisableUnsafePtrRestriction] public ExecuteSystemWrapper* Wrapper;
    }
}