using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DesertImage.ECS;

namespace DesertImage.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(UnsafeUshortSparseSet<>))]
    public unsafe struct UnsafeUshortSparseSet<T> : IDisposable, IEnumerable<T> where T : unmanaged
    {
        public bool IsNotNull { get; private set; }

        public int Count { get; private set; }

        internal T* _dense;
        internal ushort* _sparse;
        private ushort* _recycled;

        internal int _denseCapacity;
        internal int _sparseCapacity;

        private int _recycledCapacity;
        private int _recycledCount;

        public readonly T this[ushort index] => _dense[_sparse[index] - 1];

        public UnsafeUshortSparseSet(int denseCapacity, int sparseCapacity, int recycledCapacity = 100)
        {
            var ushortSize = MemoryUtility.SizeOf<ushort>();

            _dense = MemoryUtility.AllocateClear<T>(denseCapacity * MemoryUtility.SizeOf<T>());
            _sparse = MemoryUtility.AllocateClear<ushort>(sparseCapacity * ushortSize);
            _recycled = MemoryUtility.AllocateClear<ushort>(recycledCapacity * ushortSize);

            _denseCapacity = denseCapacity;
            _sparseCapacity = sparseCapacity;
            _recycledCapacity = recycledCapacity;

            Count = 0;
            _recycledCount = 0;

            IsNotNull = true;
        }

        public UnsafeUshortSparseSet(int capacity)
        {
            var ushortSize = MemoryUtility.SizeOf<ushort>();

            _dense = MemoryUtility.AllocateClear<T>(capacity * MemoryUtility.SizeOf<T>());
            _sparse = MemoryUtility.AllocateClear<ushort>(capacity * ushortSize);
            _recycled = MemoryUtility.AllocateClear<ushort>(capacity * ushortSize);

            _denseCapacity = capacity;
            _sparseCapacity = capacity;
            _recycledCapacity = capacity;

            Count = 0;
            _recycledCount = 0;

            IsNotNull = true;
        }

        public UnsafeUshortSparseSet(int denseCapacity, int sparseCapacity, int recycledCapacity, T defaultValue)
        {
            var ushortSize = MemoryUtility.SizeOf<ushort>();

            _dense = MemoryUtility.AllocateClear(denseCapacity * MemoryUtility.SizeOf<T>(), defaultValue);
            _sparse = MemoryUtility.AllocateClear<ushort>(sparseCapacity * ushortSize);
            _recycled = MemoryUtility.AllocateClear<ushort>(recycledCapacity * ushortSize);

            _denseCapacity = denseCapacity;
            _sparseCapacity = sparseCapacity;
            _recycledCapacity = recycledCapacity;

            Count = 0;
            _recycledCount = 0;

            IsNotNull = true;
        }

        public void Add(ushort key, in T value)
        {
            if (Contains(key))
            {
                _dense[_sparse[key] - 1] = value;
                return;
            }

            var targetIndex = _recycledCount > 0 ? _recycled[--_recycledCount] : (ushort)Count;

            if (key >= _sparseCapacity)
            {
                var newSparseCapacity = _sparseCapacity << 1;
                MemoryUtility.Resize(ref _sparse, _sparseCapacity, newSparseCapacity);
                _sparseCapacity = newSparseCapacity;
            }

            _sparse[key] = (ushort)(targetIndex + 1);
            _dense[targetIndex] = value;

            Count++;

            if (Count >= _denseCapacity)
            {
                var newDenseCapacity = _denseCapacity << 1;
                MemoryUtility.Resize(ref _dense, _denseCapacity, newDenseCapacity);
                _denseCapacity = newDenseCapacity;
            }
        }

        public void Remove(ushort key)
        {
            var oldSparse = _sparse[key];

            _dense[_sparse[key] - 1] = default;
            _sparse[key] = 0;

            Count--;

            AddRecycled(oldSparse);
        }

        public bool TryGetValue(ushort key, out T value)
        {
            value = default;

            if (!Contains(key)) return false;

            value = _dense[_sparse[key] - 1];

            return true;
        }

        public readonly ref T Get(ushort key) => ref _dense[_sparse[key - 1]];

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
                _sparse[i] = 0;
            }
        }

        private void AddRecycled(ushort oldSparse)
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

        public bool Contains(ushort key) => _sparseCapacity > key && _sparse[key] > 0;

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

            private readonly UnsafeUshortSparseSet<T> _sparseSet;
            private ushort _index;
            private T _current;

            public Enumerator(ref UnsafeUshortSparseSet<T> sparseSet) : this()
            {
                _sparseSet = sparseSet;
                _index = 0;
            }

            public bool MoveNext()
            {
                ++_index;
                return _index < _sparseSet.Count;
            }

            public void Reset() => _index = 0;

            public void Dispose()
            {
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(ref this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }

    internal sealed unsafe class UnsafeUshortSparseSetDebugView<T> where T : unmanaged
    {
        private readonly UnsafeUshortSparseSet<T> _data;

        public UnsafeUshortSparseSetDebugView(UnsafeUshortSparseSet<T> data) => _data = data;

        public ushort[] Sparse => MemoryUtility.ToArray(_data._sparse, _data._sparseCapacity);
        public T[] Dense => MemoryUtility.ToArray(_data._dense, _data._denseCapacity);
    }
}