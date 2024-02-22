using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DesertImage.ECS;

namespace DesertImage.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(UnsafeSparseSetDebugView<>))]
    public unsafe struct UnsafeSparseSet<T> : IDisposable, IEnumerable<T> where T : unmanaged
    {
        public bool IsNotNull { get; private set; }

        public int Count { get; private set; }

        internal T* _dense;
        internal int* _sparse;
        private int* _recycled;

        internal int _denseCapacity;
        internal int _sparseCapacity;

        private int _recycledCapacity;
        private int _recycledCount;

        public readonly T this[int index] => _dense[_sparse[index]];

        public UnsafeSparseSet(int denseCapacity, int sparseCapacity, int recycledCapacity = 100)
        {
            var intSize = MemoryUtility.SizeOf<int>();

            _dense = MemoryUtility.AllocateClear<T>(denseCapacity * MemoryUtility.SizeOf<T>());
            _sparse = MemoryUtility.AllocateClear(sparseCapacity * intSize, -1);
            _recycled = MemoryUtility.AllocateClear<int>(recycledCapacity * intSize);

            _denseCapacity = denseCapacity;
            _sparseCapacity = sparseCapacity;
            _recycledCapacity = recycledCapacity;

            Count = 0;
            _recycledCount = 0;

            IsNotNull = true;
        }

        public UnsafeSparseSet(int capacity)
        {
            var intSize = MemoryUtility.SizeOf<int>();

            _dense = MemoryUtility.AllocateClear<T>(capacity * MemoryUtility.SizeOf<T>());
            _sparse = MemoryUtility.AllocateClear(capacity * intSize, -1);
            _recycled = MemoryUtility.AllocateClear<int>(capacity * intSize);

            _denseCapacity = capacity;
            _sparseCapacity = capacity;
            _recycledCapacity = capacity;

            Count = 0;
            _recycledCount = 0;

            IsNotNull = true;
        }

        public UnsafeSparseSet(int denseCapacity, int sparseCapacity, int recycledCapacity, T defaultValue)
        {
            var intSize = MemoryUtility.SizeOf<int>();

            _dense = MemoryUtility.AllocateClear(denseCapacity * MemoryUtility.SizeOf<T>(), defaultValue);
            _sparse = MemoryUtility.AllocateClear(sparseCapacity * intSize, -1);
            _recycled = MemoryUtility.AllocateClear<int>(recycledCapacity * intSize);

            _denseCapacity = denseCapacity;
            _sparseCapacity = sparseCapacity;
            _recycledCapacity = recycledCapacity;

            Count = 0;
            _recycledCount = 0;

            IsNotNull = true;
        }

        public void Add(int key, in T value)
        {
            if (Contains(key))
            {
                _dense[_sparse[key]] = value;
                return;
            }

            var targetIndex = _recycledCount > 0 ? _recycled[--_recycledCount] : Count;

            if (key >= _sparseCapacity)
            {
                var newSparseCapacity = _sparseCapacity << 1;
                MemoryUtility.Resize(ref _sparse, _sparseCapacity, newSparseCapacity, -1);
                _sparseCapacity = newSparseCapacity;
            }

            _sparse[key] = targetIndex;
            _dense[targetIndex] = value;

            Count++;

            if (Count >= _denseCapacity)
            {
                var newDenseCapacity = _denseCapacity << 1;
                MemoryUtility.Resize(ref _dense, _denseCapacity, newDenseCapacity);
                _denseCapacity = newDenseCapacity;
            }
        }

        public void Remove(int key)
        {
            var oldSparse = _sparse[key];

            _dense[_sparse[key]] = default;
            _sparse[key] = -1;

            Count--;

            AddRecycled(oldSparse);
        }

        public readonly ref readonly T Get(int key) => ref _dense[_sparse[key]];

        public void Clear()
        {
            _recycledCount = 0;
            Count = 0;

            for (var i = 0; i < _denseCapacity; i++)
            {
                _dense[i] = default;
            }

            for (var i = 0; i < _sparseCapacity; i++)
            {
                _sparse[i] = -1;
            }
        }

        private void AddRecycled(int oldSparse)
        {
            if (_recycledCount == _recycledCapacity)
            {
                var newCapacity = _recycledCapacity << 1;
                MemoryUtility.Resize(ref _recycled, _recycledCapacity, newCapacity);
                _recycledCapacity = newCapacity;
            }

            _recycled[_recycledCount] = oldSparse;
            _recycledCount++;
        }

        public bool Contains(int key) => _sparseCapacity > key && _sparse[key] != -1;

        public readonly void Dispose()
        {
            MemoryUtility.Free(_dense);
            MemoryUtility.Free(_sparse);
            MemoryUtility.Free(_recycled);
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

    internal sealed unsafe class UnsafeSparseSetDebugView<T> where T : unmanaged
    {
        private readonly UnsafeSparseSet<T> _data;

        public UnsafeSparseSetDebugView(UnsafeSparseSet<T> data) => _data = data;

        public int[] Sparse => MemoryUtility.ToArray(_data._sparse, _data._sparseCapacity);
        public T[] Dense => MemoryUtility.ToArray(_data._dense, _data._denseCapacity);
    }
}