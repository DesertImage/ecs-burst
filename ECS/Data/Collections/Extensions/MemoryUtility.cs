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

        public static void* Allocate(long size, int alignment, Allocator allocator)
        {
#if DEBUG_MODE
            return UnsafeUtility.MallocTracked(size, alignment, allocator, 0);
#else
            return UnsafeUtility.Malloc(size, alignment, allocator);
#endif
        }

        public static T* Allocate<T>(long size, Allocator allocator = Allocator.Persistent) where T : unmanaged
        {
            return (T*)Allocate(size, UnsafeUtility.AlignOf<T>(), allocator);
        }

        public static T* Allocate<T>(Allocator allocator = Allocator.Persistent) where T : unmanaged
        {
            return (T*)Allocate(SizeOf<T>(), UnsafeUtility.AlignOf<T>(), allocator);
        }

        public static T* AllocateClear<T>(long size, Allocator allocator = Allocator.Persistent) where T : unmanaged
        {
            var ptr = Allocate<T>(size, allocator);
            UnsafeUtility.MemClear(ptr, size);
            return ptr;
        }

        public static T* AllocateClearCapacity<T>(int capacity, Allocator allocator = Allocator.Persistent)
            where T : unmanaged
        {
            var size = capacity * SizeOf<T>();
            var ptr = Allocate<T>(size, allocator);
            UnsafeUtility.MemClear(ptr, size);
            return ptr;
        }

        public static T* AllocateClear<T>(long size, T defaultValue, Allocator allocator = Allocator.Persistent)
            where T : unmanaged
        {
            var ptr = Allocate<T>(size, allocator);
            UnsafeUtility.MemClear(ptr, size);

            var length = size / SizeOf<T>();

            for (var i = 0; i < length; i++)
            {
                ptr[i] = defaultValue;
            }

            return ptr;
        }

        public static T* AllocateInstance<T>(in T instance) where T : unmanaged
        {
            var ptr = Allocate<T>(SizeOf<T>(), Allocator.Persistent);
            *ptr = instance;
            return ptr;
        }

        public static T* Resize<T>(T* ptr, int oldCapacity, int newCapacity, T defaultValue,
            Allocator allocator = Allocator.Persistent) where T : unmanaged
        {
            ptr = Resize(ptr, oldCapacity, newCapacity, allocator);

            for (var i = oldCapacity; i < newCapacity; i++)
            {
                ptr[i] = defaultValue;
            }

            return ptr;
        }

        public static T* Resize<T>(T* ptr, int oldCapacity, int newCapacity,
            Allocator allocator = Allocator.Persistent) where T : unmanaged
        {
            var elementSize = UnsafeUtility.SizeOf<T>();
            var oldSize = oldCapacity * elementSize;
            var newSize = newCapacity * elementSize;

            var newPtr = AllocateClear<T>(newSize, allocator);

            Copy(newPtr, ptr, oldSize);
            Free(ptr, allocator);

            return newPtr;
        }

        public static T* Resize<T>(T* ptr, long oldSize, long newSize, Allocator allocator = Allocator.Persistent)
            where T : unmanaged
        {
            var newPtr = AllocateClear<T>(newSize, allocator);
            Copy(newPtr, ptr, oldSize);
            Free(ptr, allocator);

            return newPtr;
        }
        
        public static void* Resize(void* ptr, long oldSize, long newSize, Allocator allocator = Allocator.Persistent)
        {
            var newPtr = AllocateClear<byte>(newSize, allocator);
            Copy(newPtr, ptr, oldSize);
            Free(ptr, allocator);

            return newPtr;
        }

        public static long SizeOf<T>() where T : unmanaged => UnsafeUtility.SizeOf<T>();

        public static void Clear<T>(T* ptr, long size) where T : unmanaged => Clear((void*)ptr, size);
        public static void Clear(void* ptr, long size) => UnsafeUtility.MemClear(ptr, size);
        public static void Clear(void** ptr, long size) => UnsafeUtility.MemClear(ptr, size);

        public static void Copy(void* destination, void* source, long size)
        {
            UnsafeUtility.MemCpy(destination, source, size);
        }

        public static void Free<T>(T* ptr, Allocator allocator = Allocator.Persistent) where T : unmanaged
        {
            Free((void*)ptr, allocator);
        }

        public static void Free(void* ptr, Allocator allocator = Allocator.Persistent)
        {
#if DEBUG_MODE
            UnsafeUtility.FreeTracked(ptr, allocator);
#else
            UnsafeUtility.Free(ptr, allocator);
#endif
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
        
        public static T*[] ToArray<T>(T** ptr, int capacity) where T : unmanaged
        {
            var array = new T*[capacity];
            for (var i = 0; i < capacity; i++)
            {
                array[i] = ptr[i];
            }

            return array;
        }
    }
}