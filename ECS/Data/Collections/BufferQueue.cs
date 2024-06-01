using System;
using System.Diagnostics;
using DesertImage.ECS;

namespace DesertImage.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(BufferQueueDebugView<>))]
    public unsafe struct BufferQueue<T> : IDisposable where T : unmanaged
    {
        public int Count { get; private set; }

        private Ptr _ptr;

        private readonly MemoryAllocator MemoryAllocator => _state->MemoryAllocator;
        private readonly WorldState* _state;

        private int _capacity;

        public BufferQueue(int capacity, WorldState* state)
        {
            _ptr = state->MemoryAllocator.Allocate(capacity * MemoryUtility.SizeOf<T>());

            _capacity = capacity;

            _state = state;

            _capacity = capacity;
            Count = 0;
        }

        public void Enqueue(T instance)
        {
            if (Count >= _capacity) Resize(_capacity << 1);
            _ptr.GetPtr<T>(MemoryAllocator)[Count] = instance;
            Count++;
        }

        public T Dequeue()
        {
#if DEBUG_MODE
            if (Count == 0) throw new ArgumentOutOfRangeException();
#endif
            var instance = _ptr.GetPtr<T>(MemoryAllocator)[Count];

            Count--;

            return instance;
        }

        public T Peek()
        {
            return Count == 0 ? default : _ptr.GetPtr<T>(MemoryAllocator)[Count];
        }

        public Ptr GetPtr() => _ptr;

        private void Resize(int newCapacity)
        {
            var elementSize = MemoryUtility.SizeOf<T>();
            var newSize = newCapacity * elementSize;

            MemoryAllocator.Resize(ref _ptr, newSize);
            // _data = (T*)_ptr.Value;
            _capacity = newCapacity;
        }

        public T[] ToArray()
        {
            var array = new T[_capacity];

            for (var i = 0; i < _capacity; i++)
            {
                array[i] = _ptr.GetPtr<T>(MemoryAllocator)[i];
            }

            return array;
        }

        public void Dispose() => MemoryAllocator.Free(_ptr);
    }

    internal sealed class BufferQueueDebugView<T> where T : unmanaged, IEquatable<T>
    {
        private BufferQueue<T> _data;

        public BufferQueueDebugView(BufferQueue<T> array) => _data = array;

        public T[] Items => _data.ToArray();
    }
}