using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DesertImage.ECS;

namespace DesertImage.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(UnsafeUintSparseSet<>))]
    public unsafe struct UnsafeUintSparseSet<T> : IDisposable, IEnumerable<T> where T : unmanaged
    {
        public bool IsNotNull { get; private set; }

        public int Count { get; private set; }

        internal T* _dense;
        internal uint* _sparse;
        private uint* _recycled;

        internal int _denseCapacity;
        internal int _sparseCapacity;

        private int _recycledCapacity;
        private int _recycledCount;

        public readonly T this[uint index] => _dense[_sparse[index] - 1];

        public UnsafeUintSparseSet(int denseCapacity, int sparseCapacity, int recycledCapacity = 100)
        {
            var uintSize = MemoryUtility.SizeOf<uint>();

            _dense = MemoryUtility.AllocateClear<T>(denseCapacity * MemoryUtility.SizeOf<T>());
            _sparse = MemoryUtility.AllocateClear<uint>(sparseCapacity * uintSize);
            _recycled = MemoryUtility.AllocateClear<uint>(recycledCapacity * uintSize);

            _denseCapacity = denseCapacity;
            _sparseCapacity = sparseCapacity;
            _recycledCapacity = recycledCapacity;

            Count = 0;
            _recycledCount = 0;

            IsNotNull = true;
        }

        public UnsafeUintSparseSet(int capacity)
        {
            var uintSize = MemoryUtility.SizeOf<uint>();

            _dense = MemoryUtility.AllocateClear<T>(capacity * MemoryUtility.SizeOf<T>());
            _sparse = MemoryUtility.AllocateClear<uint>(capacity * uintSize);
            _recycled = MemoryUtility.AllocateClear<uint>(capacity * uintSize);

            _denseCapacity = capacity;
            _sparseCapacity = capacity;
            _recycledCapacity = capacity;

            Count = 0;
            _recycledCount = 0;

            IsNotNull = true;
        }

        public UnsafeUintSparseSet(int denseCapacity, int sparseCapacity, int recycledCapacity, T defaultValue)
        {
            var uintSize = MemoryUtility.SizeOf<uint>();

            _dense = MemoryUtility.AllocateClear(denseCapacity * MemoryUtility.SizeOf<T>(), defaultValue);
            _sparse = MemoryUtility.AllocateClear<uint>(sparseCapacity * uintSize);
            _recycled = MemoryUtility.AllocateClear<uint>(recycledCapacity * uintSize);

            _denseCapacity = denseCapacity;
            _sparseCapacity = sparseCapacity;
            _recycledCapacity = recycledCapacity;

            Count = 0;
            _recycledCount = 0;

            IsNotNull = true;
        }

        public void Add(uint key, in T value)
        {
            if (Contains(key))
            {
                _dense[_sparse[key] - 1] = value;
                return;
            }

            var targetIndex = _recycledCount > 0 ? _recycled[--_recycledCount] : (uint)Count;

            if (key >= _sparseCapacity)
            {
                var newSparseCapacity = _sparseCapacity << 1;
                MemoryUtility.Resize(ref _sparse, _sparseCapacity, newSparseCapacity);
                _sparseCapacity = newSparseCapacity;
            }

            _sparse[key] = (uint)(targetIndex + 1);
            _dense[targetIndex] = value;

            Count++;

            if (Count >= _denseCapacity)
            {
                var newDenseCapacity = _denseCapacity << 1;
                MemoryUtility.Resize(ref _dense, _denseCapacity, newDenseCapacity);
                _denseCapacity = newDenseCapacity;
            }
        }

        public void Remove(uint key)
        {
            var oldSparse = _sparse[key];

            _dense[_sparse[key] - 1] = default;
            _sparse[key] = 0;

            Count--;

            AddRecycled(oldSparse);
        }

        public bool TryGetValue(uint key, out T value)
        {
            value = default;

            if (!Contains(key)) return false;

            value = _dense[_sparse[key] - 1];
            
            return true;
        }

        public readonly ref T Get(uint key) => ref _dense[_sparse[key - 1]];

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

        private void AddRecycled(uint oldSparse)
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

        public bool Contains(uint key) => _sparseCapacity > key && _sparse[key] > 0;

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

            private readonly UnsafeUintSparseSet<T> _sparseSet;
            private uint _index;
            private T _current;

            public Enumerator(ref UnsafeUintSparseSet<T> sparseSet) : this()
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
    
    internal sealed unsafe class UnsafeUintSparseSetDebugView<T> where T : unmanaged
    {
        private readonly UnsafeUintSparseSet<T> _data;

        public UnsafeUintSparseSetDebugView(UnsafeUintSparseSet<T> data) => _data = data;

        public uint[] Sparse => MemoryUtility.ToArray(_data._sparse, _data._sparseCapacity);
        public T[] Dense => MemoryUtility.ToArray(_data._dense, _data._denseCapacity);
    }
}