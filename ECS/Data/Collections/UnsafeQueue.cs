using System;
using DesertImage.ECS;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.Collections
{
    public unsafe struct UnsafeQueue<T> : IDisposable where T : unmanaged
    {
        public int Count { get; private set; }

        private T* _ptr;
        private int _lockIndex;

        private long _size;
        private int _capacity;
        private readonly Allocator _allocator;

        public UnsafeQueue(int capacity, Allocator allocator) : this()
        {
            _size = capacity * UnsafeUtility.SizeOf<T>();
            _allocator = allocator;

            _ptr = MemoryUtility.AllocateClear<T>(_size, _allocator);

            Count = 0;
            _capacity = capacity;
        }

        public void Enqueue(T element)
        {
            _lockIndex.Lock();
            {
                if (Count + 1 >= _capacity)
                {
                    Resize(Count << 1);
                }

                _ptr[Count] = element;
                Count++;
            }
            _lockIndex.Unlock();
        }

        public T Dequeue()
        {
            if (Count == 0) throw new Exception("No elements in queue");

            _lockIndex.Lock();

            var element = _ptr[Count - 1];
            Count--;

            _lockIndex.Unlock();

            return element;
        }

        public void Resize(int newCapacity)
        {
            var oldPtr = _ptr;

            var newSize = newCapacity * UnsafeUtility.SizeOf<T>();
            _ptr = MemoryUtility.AllocateClear<T>(newSize, _allocator);

            UnsafeUtility.MemClear(_ptr, newSize);
            UnsafeUtility.MemCpy(_ptr, oldPtr, _size);

            _size = newSize;
            _capacity = newCapacity;

            MemoryUtility.Free(oldPtr, _allocator);
        }

        public void Clear()
        {
            _lockIndex.Lock();
            {
                UnsafeUtility.MemClear(_ptr, _size);
                Count = 0;
            }
            _lockIndex.Unlock();
        }

        public void Dispose() => MemoryUtility.Free(_ptr, _allocator);
    }
}