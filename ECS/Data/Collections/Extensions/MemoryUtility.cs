using System;
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

        public static void ShiftLeft<T>(ref T* array, int startIndex, int length) where T : unmanaged
        {
            for (var i = startIndex; i < length - 1; i++)
            {
                array[i] = array[i + 1];
            }
        }

        public static bool IsNull(byte* data, int offset, uint index, long elementSize)
        {
            var indexOffset = index * elementSize;
            return data + offset + indexOffset == null;
        }

        public static T* Allocate<T>() where T : unmanaged
        {
            return (T*)UnsafeUtility.Malloc(SizeOf<T>(), 0, Allocator.Persistent);
        }

        public static T* AllocateClear<T>(long size, Allocator allocator = Allocator.Persistent) where T : unmanaged
        {
            var ptr = (T*)UnsafeUtility.Malloc(size, UnsafeUtility.AlignOf<T>(), allocator);
            UnsafeUtility.MemClear(ptr, size);
            return ptr;
        }

        public static T* AllocateClear<T>(long size, T defaultValue, Allocator allocator = Allocator.Persistent)
            where T : unmanaged
        {
            var ptr = (T*)UnsafeUtility.Malloc(size, UnsafeUtility.AlignOf<T>(), allocator);
            UnsafeUtility.MemClear(ptr, size);

            var length = size / SizeOf<T>();

            for (var i = 0; i < length; i++)
            {
                ptr[i] = defaultValue;
            }

            return ptr;
        }

        public static T* Allocate<T>(T instance) where T : unmanaged
        {
            var ptr = (T*)UnsafeUtility.Malloc(SizeOf<T>(), UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
            *ptr = instance;
            return ptr;
        }

        public static T* Resize<T>(ref T* ptr, int oldCapacity, int newCapacity, T defaultValue,
            Allocator allocator = Allocator.Persistent) where T : unmanaged
        {
            Resize(ref ptr, oldCapacity, newCapacity, allocator);

            for (var i = oldCapacity; i < newCapacity; i++)
            {
                ptr[i] = defaultValue;
            }

            return ptr;
        }

        public static T* Resize<T>(ref T* ptr, int oldCapacity, int newCapacity,
            Allocator allocator = Allocator.Persistent) where T : unmanaged
        {
            var oldPtr = ptr;

            var elementSize = UnsafeUtility.SizeOf<T>();
            var oldSize = oldCapacity * elementSize;
            var newSize = newCapacity * elementSize;

            ptr = (T*)UnsafeUtility.Malloc(newSize, UnsafeUtility.AlignOf<T>(), allocator);

            UnsafeUtility.MemClear(ptr, newSize);
            UnsafeUtility.MemCpy(ptr, oldPtr, oldSize);
            UnsafeUtility.Free(oldPtr, allocator);

            return ptr;
        }

        public static T* Resize<T>(ref T* ptr, long oldSize, long newSize, Allocator allocator = Allocator.Persistent)
            where T : unmanaged
        {
            var oldPtr = ptr;

            ptr = (T*)UnsafeUtility.Malloc(newSize, UnsafeUtility.AlignOf<T>(), allocator);

            UnsafeUtility.MemClear(ptr, newSize);
            UnsafeUtility.MemCpy(ptr, oldPtr, oldSize);
            UnsafeUtility.Free(oldPtr, allocator);

            return ptr;
        }

        public static long SizeOf<T>() where T : unmanaged => UnsafeUtility.SizeOf<T>();

        public static void Clear<T>(ref T* ptr, long size) where T : unmanaged => UnsafeUtility.MemClear(ptr, size);

        public static void Free<T>(T* ptr, Allocator allocator = Allocator.Persistent) where T : unmanaged
        {
            UnsafeUtility.Free(ptr, allocator);
        }

        public static void Free(void* ptr, Allocator allocator = Allocator.Persistent)
        {
            UnsafeUtility.Free(ptr, allocator);
        }

        public static T[] ToArray<T>(T* ptr, int capacity) where T : unmanaged
        {
            var array = new T[capacity];
            for (var i = 0; i < capacity; i++)
            {
                array[i] = ptr[i];
            }

            return array;
        }
    }
}