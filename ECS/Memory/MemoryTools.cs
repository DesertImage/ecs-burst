using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    internal static unsafe class MemoryTools
    {
        public static void* Allocate<T>(Allocator allocator = Allocator.Persistent) where T : unmanaged
        {
            return UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>(), 0, allocator);
        }

        public static void* Allocate<T>(T instance, Allocator allocator = Allocator.Persistent) where T : unmanaged
        {
            var ptr = Allocate<T>(allocator);
            *(T*)ptr = instance;
            return ptr;
        }

        public static void Free(void* ptr, Allocator allocator = Allocator.Persistent)
        {
            UnsafeUtility.Free(ptr, allocator);
        }
    }
}