using System;

namespace DesertImage.ECS
{
    public unsafe struct Ptr : IEquatable<Ptr>
    {
        public int Id;
        // public void* Value;

#if DEBUG_MODE
        internal long Size;
#endif

        public readonly T* GetPtr<T>(in MemoryAllocator allocator) where T : unmanaged => (T*)GetPtr(allocator);
        public readonly void* GetPtr(in MemoryAllocator allocator) => allocator.GetPtr(this);
        
#if DEBUG_MODE
        public Ptr(int id, long size)
        {
            Id = id;
            // Value = ptr;
            Size = size;
        }
#else
        public Ptr(int id)
        {
            Id = id;
        }
#endif

        public void Set<T>(T value, in MemoryAllocator allocator) where T : unmanaged
        {
#if DEBUG_MODE
            var typeSize = MemoryUtility.SizeOf<T>();
            if (typeSize > Size) throw new Exception("Ptr size mismatch");
#endif
            *GetPtr<T>(allocator) = value;
        }

        public void Set<T>(int index, T value, in MemoryAllocator allocator) where T : unmanaged
        {
#if DEBUG_MODE
            var typeSize = MemoryUtility.SizeOf<T>();
            if (typeSize * index > Size) throw new Exception("out of ptr range");
#endif
            GetPtr<T>(allocator)[index] = value;
        }

        public T Read<T>(in MemoryAllocator allocator) where T : unmanaged
        {
#if DEBUG_MODE
            var typeSize = MemoryUtility.SizeOf<T>();
            if (typeSize > Size) throw new Exception("Ptr size mismatch");
#endif
            return *GetPtr<T>(allocator);
        }

        public T Read<T>(int index, in MemoryAllocator allocator) where T : unmanaged
        {
#if DEBUG_MODE
            var typeSize = MemoryUtility.SizeOf<T>();
            if (typeSize * index > Size) throw new Exception("out of ptr range");
#endif
            return GetPtr<T>(allocator)[index];
        }

        public ref T Get<T>(in MemoryAllocator allocator) where T : unmanaged
        {
#if DEBUG_MODE
            var typeSize = MemoryUtility.SizeOf<T>();
            if (typeSize > Size) throw new Exception("Ptr size mismatch");
#endif
            return ref *GetPtr<T>(allocator);
        }

        public ref T Get<T>(int index, in MemoryAllocator allocator) where T : unmanaged
        {
#if DEBUG_MODE
            var typeSize = MemoryUtility.SizeOf<T>();
            if (typeSize * index > Size) throw new Exception("out of ptr range");
#endif
            return ref GetPtr<T>(allocator)[index];
        }

        public bool Equals(Ptr other) => Id == other.Id;
    }
}