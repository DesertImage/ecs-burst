using System;
using System.Collections;
using System.Collections.Generic;
using DesertImage.ECS;
using Unity.Collections;

namespace DesertImage.Collections
{
    public struct UnsafeSparseSet<T> : IDisposable, IEnumerable<T> where T : unmanaged
    {
        public bool IsNotNull { get; private set; }
        
        public int Count
        {
            get => _denseCount;
            private set => _denseCount = value;
        }

        private UnsafeArray<T> _dense;
        private UnsafeArray<int> _sparse;
        private UnsafeArray<int> _recycled;

        private int _recycledCount;
        private int _denseCount;

        public ref readonly T this[int index] => ref _dense.Get(index);

        public UnsafeSparseSet(int denseCapacity, int sparseCapacity, int recycledCapacity = 100) : this()
        {
            _dense = new UnsafeArray<T>(denseCapacity, Allocator.Persistent);
            _recycled = new UnsafeArray<int>(recycledCapacity, Allocator.Persistent);

            _sparse = new UnsafeArray<int>(sparseCapacity, Allocator.Persistent);
            for (var i = 0; i < _sparse.Length; i++)
            {
                _sparse[i] = -1;
            }

            IsNotNull = true;
        }
        
        public UnsafeSparseSet(int denseCapacity, int sparseCapacity, int recycledCapacity, T defaultValue) : this()
        {
            _dense = new UnsafeArray<T>(denseCapacity, Allocator.Persistent);
            for (var i = 0; i < _dense.Length; i++)
            {
                _dense[i] = defaultValue;
            }
            
            _recycled = new UnsafeArray<int>(recycledCapacity, Allocator.Persistent);

            _sparse = new UnsafeArray<int>(sparseCapacity, Allocator.Persistent);
            for (var i = 0; i < _sparse.Length; i++)
            {
                _sparse[i] = -1;
            }

            IsNotNull = true;
        }

        public void Add(int index, in T value)
        {
            if (Contains(index))
            {
                _dense[_sparse[index]] = value;
                return;
            }

            var targetIndex = _recycledCount > 0 ? _recycled[--_recycledCount] : _denseCount;

            if (index >= _sparse.Length)
            {
                //TODO: resize
                throw new Exception("Array need to be resized");
                // Array.Resize(ref _sparse, _sparse.Length << 1);
            }

            _sparse[index] = targetIndex;
            _dense[targetIndex] = value;

            _denseCount++;
        }

        public void Remove(int index)
        {
            var oldSparse = _sparse[index];

            _dense[_sparse[index]] = default;
            _sparse[index] = -1;

            _denseCount--;

            AddRecycled(oldSparse);
        }

        public readonly unsafe ref readonly T Get(int index) => ref _dense.Get(_sparse[index]);

        public void Clear()
        {
            _recycledCount = 0;
            _denseCount = 0;

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

        public void Dispose()
        {
            _dense.Dispose();
            _sparse.Dispose();
            _recycled.Dispose();
        }

        public struct Enumerator : IEnumerator<T>
        {
            object IEnumerator.Current => Current;

            public T Current => _sparseSet.Get(_index);

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
                return _index < _sparseSet._denseCount;
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