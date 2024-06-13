using System;
using System.Diagnostics;
using DesertImage.ECS;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(UnsafeQueueDebugView<>))]
    public unsafe struct UnsafeQueue<T> : IDisposable where T : unmanaged
    {
        public int Count { get; private set; }

        [NativeDisableUnsafePtrRestriction] private T* _ptr;

        private long _size;
        internal int _capacity;
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
            if (Count >= _capacity)
            {
                Resize(Count << 1);
            }

            // _ptr[_capacity - 1 - Count] = element;
            _ptr[Count] = element;
            
            Count++;
        }

        public T Dequeue()
        {
            if (Count == 0) throw new Exception("No elements in queue");

            // var index = (_capacity - 1) - (Count - 1);
            var element = _ptr[0];

            MemoryUtility.ShiftLeft(ref _ptr, 0, Count);
            
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
                _ptr[i] = oldPtr[i];
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

    internal unsafe sealed class UnsafeQueueDebugView<T> where T : unmanaged
    {
        private UnsafeQueue<T> _data;

        public UnsafeQueueDebugView(UnsafeQueue<T> array) => _data = array;

        public T[] Items => _data.ToArray();
        public T[] FullArray => MemoryUtility.ToArray((T*)_data.GetPtr(), _data._capacity);
    }
}