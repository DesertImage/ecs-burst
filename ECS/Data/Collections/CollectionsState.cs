using System;
using DesertImage.ECS;
using Unity.Collections;

namespace DesertImage.Collections
{
    public unsafe struct CollectionsState : IDisposable
    {
        private struct MemorySlice : IEquatable<MemorySlice>
        {
            public void* Ptr;
            public long Size;

            public bool Equals(MemorySlice other)
            {
                return Size == other.Size && Ptr == other.Ptr;
            }
        }

        private byte* _buffer;
        private long _size;
        private long _lastOffset;

        private UnsafeList<MemorySlice> _cached;

        public CollectionsState(long size)
        {
            _buffer = MemoryUtility.AllocateClear<byte>(size);

            _cached = new UnsafeList<MemorySlice>(10, Allocator.Persistent);

            _size = size;
            _lastOffset = 0;
        }

        public void* Get(long size)
        {
            for (var i = _cached.Count - 1; i >= 0; i--)
            {
                var slice = _cached[i];
                if (slice.Size != size) continue;

                _cached.RemoveAt(i);
                return slice.Ptr;
            }

            var offset = _lastOffset + size;

            if (offset >= _size) Resize(_size << 2, offset);

            _lastOffset = offset;

            return _buffer + offset;
        }

        public void Free(void* ptr, long size)
        {
            _cached.Add
            (
                new MemorySlice
                {
                    Ptr = ptr,
                    Size = size
                }
            );
        }

        private void Resize(long newSize, long minSize)
        {
            if (newSize < minSize)
            {
                newSize = minSize + 2;
            }

            _buffer = MemoryUtility.Resize(_buffer, _size, newSize);
            _size = newSize;
        }

        public void Dispose()
        {
            MemoryUtility.Free(_buffer);
            _cached.Dispose();
        }
    }
}