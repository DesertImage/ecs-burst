using System;
using System.Diagnostics;
using DesertImage.ECS;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(BufferListDebugView<>))]
    public unsafe struct BufferList<T> : IDisposable where T : unmanaged
    {
        public int Count { get; private set; }
        public int Capacity => _capacity;

        private Ptr _ptr;

        private readonly MemoryAllocator _memoryAllocator => _state->MemoryAllocator;
        private WorldState* _state;

        private int _capacity;

        public BufferList(int capacity, WorldState* state)
        {
            _ptr = state->MemoryAllocator.Allocate(capacity * MemoryUtility.SizeOf<T>());
            // _data = (T*)_ptr.Value;

            _capacity = capacity;
            // _memoryAllocator = allocator;

            _state = state;
            
            _capacity = capacity;
            Count = 0;
        }

        public void Add(T instance)
        {
            if (Count >= _capacity) Resize(_capacity << 1);
            _ptr.GetPtr<T>(_memoryAllocator)[Count] = instance;
            Count++;
        }

        public void RemoveAt(int index)
        {
            var data = _ptr.GetPtr<T>(_memoryAllocator);
            MemoryUtility.ShiftLeft(ref data, index, Count);
            Count--;
        }

        public readonly T Read(int index)
        {
#if DEBUG_MODE
            if (index >= _capacity) throw new ArgumentOutOfRangeException();
#endif
            return _ptr.GetPtr<T>(_memoryAllocator)[index];
        }

        public ref T Get(int index)
        {
#if DEBUG_MODE
            if (index >= _capacity) throw new ArgumentOutOfRangeException();
#endif
            return ref _ptr.GetPtr<T>(_memoryAllocator)[index];
        }

        public Ptr GetPtr() => _ptr;

        private void Resize(int newCapacity)
        {
            var elementSize = MemoryUtility.SizeOf<T>();
            var newSize = newCapacity * elementSize;

            _memoryAllocator.Resize(ref _ptr, newSize);
            // _data = (T*)_ptr.Value;
            _capacity = newCapacity;
        }
        
        public T[] ToArray()
        {
            var array = new T[_capacity];

            for (var i = 0; i < _capacity; i++)
            {
                array[i] = _ptr.GetPtr<T>(_memoryAllocator)[i];
            }

            return array;
        }

        public void Dispose()
        {
            _memoryAllocator.Free(_ptr);
        }

        public T this[int index]
        {
            get => _ptr.GetPtr<T>(_memoryAllocator)[index];
            set => _ptr.GetPtr<T>(_memoryAllocator)[index] = value;
        }
    }
    
    internal sealed class BufferListDebugView<T> where T : unmanaged, IEquatable<T>
    {
        private BufferList<T> _data;

        public BufferListDebugView(BufferList<T> array) => _data = array;

        public T[] Items => _data.ToArray();
    }
}