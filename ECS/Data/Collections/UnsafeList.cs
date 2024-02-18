using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;

namespace DesertImage.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(UnsafeListDebugView<>))]
    public unsafe struct UnsafeList<T> : IDisposable, IEnumerable<T> where T : unmanaged
    {
        public bool IsNull => _array.IsNull;

        public int Count { get; private set; }

        private UnsafeArray<T> _array;

        private int _capacity;

        public UnsafeList(int capacity, Allocator allocator) : this()
        {
            _array = new UnsafeArray<T>(capacity, allocator);
            _capacity = capacity;
        }

        public UnsafeList(int capacity, Allocator allocator, T defaultValue) : this()
        {
            _array = new UnsafeArray<T>(capacity, allocator, defaultValue);
            _capacity = capacity;
        }

        public void Add(T element)
        {
            if (Count >= _capacity)
            {
                _capacity <<= 1;

                _array.Resize(_capacity);
            }

            _array[Count] = element;

            Count++;
        }

        public void Remove(T element)
        {
            if (!Contains(element))
            {
#if DEBUG
                throw new Exception($"not contains {element}");
#endif
                return;
            }

            RemoveAt(IndexOf(element));
        }

        public void RemoveAt(int index)
        {
            _array[index] = default;
            _array.ShiftLeft(index);
        }

        public void Clear()
        {
            _array.Clear();
            Count = 0;
        }

        public bool Contains(T element)
        {
            for (var i = 0; i < _array.Length; i++)
            {
                if (element.Equals(_array[i])) return true;
            }

            return false;
        }

        public int IndexOf(T element)
        {
            for (var i = 0; i < _array.Length; i++)
            {
                if (element.Equals(_array[i])) return i;
            }

            return -1;
        }

        public ref T GetByRef(int index) => ref _array.Get(index);

        public T[] ToArray() => _array.ToArray();
        
        public struct Enumerator : IEnumerator<T>
        {
            object IEnumerator.Current => Current;

            public T Current => _current;

            private readonly UnsafeList<T> _list;
            private int _index;
            private T _current;

            public Enumerator(ref UnsafeList<T> list) : this()
            {
                _list = list;
                _index = -1;
            }

            public bool MoveNext()
            {
                ++_index;
                return _index < _list.Count;
            }

            public void Reset() => _index = -1;

            public void Dispose()
            {
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(ref this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        public readonly void Dispose() => _array.Dispose();

        public T this[int index]
        {
            get => _array[index];
            set => _array[index] = value;
        }
    }

    internal sealed class UnsafeListDebugView<T> where T : unmanaged
    {
        private UnsafeList<T> _data;

        public UnsafeListDebugView(UnsafeList<T> array) => _data = array;

        public T[] Items => _data.ToArray();
    }
}