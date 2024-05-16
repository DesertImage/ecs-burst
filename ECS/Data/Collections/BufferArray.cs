using System;
using System.Diagnostics;
using DesertImage.ECS;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.Collections
{
    [DebuggerDisplay("Count = {Length}")]
    [DebuggerTypeProxy(typeof(BufferArrayDebugView<>))]
    public unsafe struct BufferArray<T> : IDisposable where T : unmanaged
    {
        public int Length { get; private set; }

        private Ptr _ptr;

        private readonly MemoryAllocator _memoryAllocator => _state->MemoryAllocator;
        private WorldState* _state;

        private int _length;

        public BufferArray(int length, WorldState* state)
        {
            _ptr = state->MemoryAllocator.Allocate(length * MemoryUtility.SizeOf<T>());

            _length = length;

            _state = state;
            
            _length = length;
            Length = 0;
        }

        public readonly T Read(int index)
        {
#if DEBUG_MODE
            if (index >= _length) throw new ArgumentOutOfRangeException();
#endif
            return _ptr.GetPtr<T>(_memoryAllocator)[index];
        }

        public ref T Get(int index)
        {
#if DEBUG_MODE
            if (index >= _length) throw new ArgumentOutOfRangeException();
#endif
            return ref _ptr.GetPtr<T>(_memoryAllocator)[index];
        }

        public Ptr GetPtr() => _ptr;

        public T[] ToArray()
        {
            var array = new T[_length];

            for (var i = 0; i < _length; i++)
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
    
    internal sealed class BufferArrayDebugView<T> where T : unmanaged, IEquatable<T>
    {
        private BufferArray<T> _data;

        public BufferArrayDebugView(BufferArray<T> array) => _data = array;

        public T[] Items => _data.ToArray();
    }
}