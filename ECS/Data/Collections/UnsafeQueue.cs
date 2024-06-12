using System;
using DesertImage.ECS;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.Collections
{
    public unsafe struct UnsafeQueue<T> : IDisposable where T : unmanaged
    {
        public int Count { get; private set; }

        [NativeDisableUnsafePtrRestriction] private T* _ptr;

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
            if (_capacity - Count <= 0)
            {
                Resize(Count << 1);
            }

            _ptr[_capacity - Count] = element;
            Count++;
        }

        public T Dequeue()
        {
            if (Count == 0) throw new Exception("No elements in queue");

            var element = _ptr[_capacity - Count];
            Count--;

            return element;
        }

        public void* GetPtr() => _ptr;
        
        public void Resize(int newCapacity)
        {
            var oldPtr = _ptr;

            var elementSize = UnsafeUtility.SizeOf<T>();
            long newSize = newCapacity * elementSize;
            _ptr = MemoryUtility.AllocateClear<T>(newSize, _allocator);

            UnsafeUtility.MemClear(_ptr, newSize);

            for (var i = 0; i < _capacity; i++)
            {
                _ptr[newCapacity - i] = oldPtr[_capacity - i];
            }
            
            _size = newSize;
            _capacity = newCapacity;

            MemoryUtility.Free(oldPtr, _allocator);
        }

        public void Clear()
        {
            UnsafeUtility.MemClear(_ptr, _size);
            Count = 0;
        }

        public void Dispose() => MemoryUtility.Free(_ptr, _allocator);
    }
}