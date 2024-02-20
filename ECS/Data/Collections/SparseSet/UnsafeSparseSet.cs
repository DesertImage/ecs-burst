using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

namespace DesertImage.Collections
{
    public struct UnsafeSparseSet<T> : IDisposable, IEnumerable<T> where T : unmanaged
    {
        public bool IsNotNull { get; private set; }

        public int Count { get; private set; }

        private UnsafeArray<T> _dense;
        private UnsafeArray<int> _sparse;
        private UnsafeArray<int> _recycled;

        private int _recycledCount;

        public ref readonly T this[int index] => ref _dense.Get(_sparse[index]);

        public UnsafeSparseSet(int denseCapacity, int sparseCapacity, int recycledCapacity = 100) : this()
        {
            _dense = new UnsafeArray<T>(denseCapacity, Allocator.Persistent, default);
            _sparse = new UnsafeArray<int>(sparseCapacity, Allocator.Persistent, -1);
            _recycled = new UnsafeArray<int>(recycledCapacity, Allocator.Persistent, default);

            IsNotNull = true;
        }

        public UnsafeSparseSet(int capacity)
        {
            _dense = new UnsafeArray<T>(capacity, Allocator.Persistent, default);
            _sparse = new UnsafeArray<int>(capacity, Allocator.Persistent, -1);
            _recycled = new UnsafeArray<int>(capacity, Allocator.Persistent, default);

            IsNotNull = true;
            Count = 0;
            _recycledCount = 0;
        }

        public UnsafeSparseSet(int denseCapacity, int sparseCapacity, int recycledCapacity, T defaultValue)
        {
            _dense = new UnsafeArray<T>(denseCapacity, Allocator.Persistent, defaultValue);
            _sparse = new UnsafeArray<int>(sparseCapacity, Allocator.Persistent, -1);
            _recycled = new UnsafeArray<int>(recycledCapacity, Allocator.Persistent);

            IsNotNull = true;
            Count = 0;
            _recycledCount = 0;
        }

        public void Add(int index, in T value)
        {
            if (Contains(index))
            {
                _dense[_sparse[index]] = value;
                return;
            }

            var targetIndex = _recycledCount > 0 ? _recycled[--_recycledCount] : Count;

            if (index >= _sparse.Length)
            {
                _sparse.Resize(_sparse.Length << 1);
            }

            _sparse[index] = targetIndex;
            _dense[targetIndex] = value;

            Count++;

            if (Count >= _dense.Length)
            {
                _dense.Resize(Count << 1);
            }
        }

        public void Remove(int index)
        {
            var oldSparse = _sparse[index];

            _dense[_sparse[index]] = default;
            _sparse[index] = -1;

            Count--;

            AddRecycled(oldSparse);
        }

        public readonly unsafe ref readonly T Get(int index) => ref _dense.Get(_sparse[index]);

        public void Clear()
        {
            _recycledCount = 0;
            Count = 0;

            for (var i = 0; i < _dense.Length; i++)
            {
                _dense[i] = default;
            }

            for (var i = 0; i < _sparse.Length; i++)
            {
                _sparse[i] = -1;
            }
        }

        private void AddRecycled(int oldSparse)
        {
            if (_recycledCount == _recycled.Length)
            {
                throw new Exception("Array need to be resized");
            }

            _recycled[_recycledCount] = oldSparse;
            _recycledCount++;
        }

        public bool Contains(int index) => _sparse.Length > index && _sparse[index] != -1;

        public readonly void Dispose()
        {
            _dense.Dispose();
            _sparse.Dispose();
            _recycled.Dispose();
        }

        public struct Enumerator : IEnumerator<T>
        {
            object IEnumerator.Current => Current;

            public T Current => _sparseSet._dense[_index];

            private readonly UnsafeSparseSet<T> _sparseSet;
            private int _index;
            private T _current;

            public Enumerator(ref UnsafeSparseSet<T> sparseSet) : this()
            {
                _sparseSet = sparseSet;
                _index = -1;
            }

            public bool MoveNext()
            {
                ++_index;
                return _index < _sparseSet.Count;
            }

            public void Reset() => _index = -1;

            public void Dispose()
            {
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(ref this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }
}