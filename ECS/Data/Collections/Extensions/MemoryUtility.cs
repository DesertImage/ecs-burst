using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public static class MemoryUtility
    {
        private struct MemoryCache<T>
        {
            public static int Size;
        }

        public static unsafe ref T Read<T>(byte* data, int offset, int index) where T : unmanaged
        {
            return ref Read<T>(data, offset, index, UnsafeUtility.SizeOf<T>());
        }

        public static unsafe ref T Read<T>(byte* data, int offset, int index, long elementSize) where T : unmanaged
        {
            var indexOffset = index * elementSize;
            return ref *(T*)(data + offset + indexOffset);
        }

        public static unsafe bool IsNull<T>(byte* data, int offset, int index, long elementSize) where T : unmanaged
        {
            var indexOffset = index * elementSize;
            return (IntPtr)(data + offset + indexOffset) != IntPtr.Zero;
        }

        public static unsafe ref T Read<T>(T* data, int index) where T : unmanaged
        {
            var indexOffset = index * UnsafeUtility.SizeOf<T>();
            return ref *(data + indexOffset);
        }

        public static unsafe ref T Read<T>(T* data, int index, long elementSize) where T : unmanaged
        {
            var indexOffset = index * elementSize;
            return ref *(data + indexOffset);
        }

        public static unsafe void Write<T>(byte* data, int offset, int index, T instance) where T : struct
        {
            Write(data, offset, index, instance, UnsafeUtility.SizeOf<T>());
        }

        public static unsafe void Write<T>(byte* data, int offset, int index, T instance, long elementSize)
            where T : struct
        {
            var indexOffset = index * elementSize;
            UnsafeUtility.CopyStructureToPtr(ref instance, data + offset + indexOffset);
        }

        public static unsafe void Write<T>(T* data, int offset, int index, T instance, long elementSize)
            where T : unmanaged
        {
            var indexOffset = index * elementSize;
            UnsafeUtility.CopyStructureToPtr(ref instance, data + offset + indexOffset);
        }

        public static unsafe void Write<T>(T* data, int index, T value) where T : unmanaged
        {
            Write(data, index, value, UnsafeUtility.SizeOf<T>());
        }

        public static unsafe void Write<T>(T* data, int index, T value, long elementSize) where T : unmanaged
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

        public static unsafe T* Allocate<T>() where T : unmanaged
        {
            return (T*)UnsafeUtility.Malloc(GetSize<T>(), 0, Allocator.Persistent);
        }

        public static unsafe T* Allocate<T>(T instance) where T : unmanaged
        {
            var ptr = (T*)UnsafeUtility.Malloc(GetSize<T>(), 0, Allocator.Persistent);
            *ptr = instance;
            return ptr;
        }
    }
}