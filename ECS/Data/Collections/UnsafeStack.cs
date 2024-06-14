using System;
using System.Diagnostics;
using DesertImage.ECS;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(UnsafeStackDebugView<>))]
    public unsafe struct UnsafeStack<T> : IDisposable where T : unmanaged
    {
        public int Count { get; private set; }

        [NativeDisableUnsafePtrRestriction] private T* _ptr;

        private long _size;
        internal int _capacity;
        private readonly Allocator _allocator;

        public UnsafeStack(int capacity, Allocator allocator) : this()
        {
            _size = capacity * UnsafeUtility.SizeOf<T>();
            _allocator = allocator;

            _ptr = MemoryUtility.AllocateClear<T>(_size, _allocator);

            Count = 0;
            _capacity = capacity;
        }

        public void Push(T element)
        {
            if (Count + 1 >= _capacity)
            {
                Resize(Count << 1);
            }

            _ptr[Count] = element;
            Count++;
        }

        public T Pull()
        {
            if (Count == 0) throw new Exception("No elements in Stack");

            var element = _ptr[Count - 1];
            Count--;

            return element;
        }

        public void Resize(int newCapacity)
        {
            var oldPtr = _ptr;

            long newSize = newCapacity * UnsafeUtility.SizeOf<T>();
            _ptr = MemoryUtility.AllocateClear<T>(newSize, _allocator);

            UnsafeUtility.MemClear(_ptr, newSize);
            MemoryUtility.Copy(_ptr, oldPtr, _size);

            _size = newSize;
            _capacity = newCapacity;

            MemoryUtility.Free(oldPtr, _allocator);
        }
        
        public void* GetPtr() => _ptr;

        public void Clear()
        {
            UnsafeUtility.MemClear(_ptr, _size);
            Count = 0;
        }

        public void Dispose() => MemoryUtility.Free(_ptr, _allocator);
    }

    internal unsafe sealed class UnsafeStackDebugView<T> where T : unmanaged
    {
        private UnsafeStack<T> _data;

        public UnsafeStackDebugView(UnsafeStack<T> array) => _data = array;

        public T[] Items => _data.ToArray();
        public T[] FullArray => MemoryUtility.ToArray((T*)_data.GetPtr(), _data._capacity);
    }
}