using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public unsafe static class MemoryUtility
    {
        private struct MemoryCache<T>
        {
            public static int Size;
        }

        public static ref T Read<T>(byte* data, int offset, uint index) where T : unmanaged
        {
            return ref Read<T>(data, offset, index, UnsafeUtility.SizeOf<T>());
        }

        public static ref T Read<T>(byte* data, int offset, uint index, long elementSize) where T : unmanaged
        {
            var indexOffset = index * elementSize;
            return ref *(T*)(data + offset + indexOffset);
        }

        public static bool IsNull(byte* data, int offset, uint index, long elementSize)
        {
            var indexOffset = index * elementSize;
            return data + offset + indexOffset == null;
        }

        public static ref T Read<T>(T* data, uint index) where T : unmanaged
        {
            var indexOffset = index * UnsafeUtility.SizeOf<T>();
            return ref *(data + indexOffset);
        }

        public static ref T Read<T>(T* data, uint index, long elementSize) where T : unmanaged
        {
            var indexOffset = index * elementSize;
            return ref *(data + indexOffset);
        }

        public static void Write<T>(byte* data, int offset, uint index, T instance) where T : struct
        {
            Write(data, offset, index, instance, UnsafeUtility.SizeOf<T>());
        }

        public static void Write<T>(byte* data, int offset, uint index, T instance, long elementSize)
            where T : struct
        {
            var indexOffset = index * elementSize;
            UnsafeUtility.CopyStructureToPtr(ref instance, data + offset + indexOffset);
        }

        public static void Write<T>(T* data, int offset, uint index, T instance, long elementSize)
            where T : unmanaged
        {
            var indexOffset = index * elementSize;
            UnsafeUtility.CopyStructureToPtr(ref instance, data + offset + indexOffset);
        }

        public static void Write<T>(T* data, uint index, T value) where T : unmanaged
        {
            Write(data, index, value, UnsafeUtility.SizeOf<T>());
        }

        public static void Write<T>(T* data, uint index, T value, long elementSize) where T : unmanaged
        {
            var indexOffset = index * elementSize;
            UnsafeUtility.CopyStructureToPtr(ref value, data + indexOffset);
        }

        public static int GetSize<T>() where T : struct
        {
            return UnsafeUtility.SizeOf<T>();

            var size = MemoryCache<T>.Size;

            if (size > 0) return size;

            size = UnsafeUtility.SizeOf<T>();

            MemoryCache<T>.Size = size;

            return size;
        }

        public static T* Allocate<T>() where T : unmanaged
        {
            return (T*)UnsafeUtility.Malloc(GetSize<T>(), 0, Allocator.Persistent);
        }

        public static T* Allocate<T>(T instance) where T : unmanaged
        {
            var ptr = (T*)UnsafeUtility.Malloc(GetSize<T>(), 0, Allocator.Persistent);
            *ptr = instance;
            return ptr;
        }

        public static void Free<T>(T* ptr, Allocator allocator = Allocator.Persistent) where T : unmanaged
        {
            UnsafeUtility.Free(ptr, allocator);
        }

        public static void Free(void* ptr, Allocator allocator = Allocator.Persistent)
        {
            UnsafeUtility.Free(ptr, allocator);
        }
    }
}