using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DesertImage.ECS;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(UnsafeUintReadOnlySparseSetDebugView<>))]
    public unsafe struct UnsafeUintReadOnlySparseSet<T> : IEnumerable<T> where T : unmanaged
    {
        public bool IsNotNull { get; private set; }

        public int Count { get; private set; }

        public UnsafeReadOnlyArray<T> Values => new UnsafeReadOnlyArray<T>(_dense, Count);
        public UnsafeReadOnlyArray<uint> Keys => new UnsafeReadOnlyArray<uint>(_keys, Count);

        [NativeDisableUnsafePtrRestriction] internal T* _dense;
        [NativeDisableUnsafePtrRestriction] internal uint* _sparse;
        [NativeDisableUnsafePtrRestriction] internal uint* _keys;

        internal int _denseCapacity;
        internal int _sparseCapacity;

        public readonly T this[uint index] => _dense[_sparse[index] - 1];

        public UnsafeUintReadOnlySparseSet(T* dense, int denseCapacity, uint* sparse, int sparseCapacity, uint* keys,
            int count)
        {
            _dense = dense;
            _sparse = sparse;
            _keys = keys;

            Count = count;

            _denseCapacity = denseCapacity;
            _sparseCapacity = sparseCapacity;

            IsNotNull = true;
        }

        public bool TryGetValue(uint key, out T value)
        {
            value = default;

            if (!Contains(key)) return false;

            value = _dense[_sparse[key] - 1];

            return true;
        }

        public readonly ref T Get(uint key) => ref _dense[_sparse[key] - 1];
        public readonly T Read(uint key) => _dense[_sparse[key] - 1];

        public bool Contains(uint key)
        {
            if (key >= _sparseCapacity) return false;
            return _sparseCapacity > key && _sparse[key] > 0;
        }

        public struct Enumerator : IEnumerator<T>
        {
            object IEnumerator.Current => Current;

            public T Current => _sparseSet._dense[_index - 1];

            private readonly UnsafeUintReadOnlySparseSet<T> _sparseSet;
            private uint _index;
            private uint _counter;
            private T _current;

            public Enumerator(ref UnsafeUintReadOnlySparseSet<T> sparseSet) : this()
            {
                _sparseSet = sparseSet;
                _index = 0;
            }

            public bool MoveNext()
            {
                ++_index;
                return _index - 1 < _sparseSet.Count;
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

    internal sealed unsafe class UnsafeUintReadOnlySparseSetDebugView<T> where T : unmanaged
    {
        private readonly UnsafeUintReadOnlySparseSet<T> _data;

        public UnsafeUintReadOnlySparseSetDebugView(UnsafeUintReadOnlySparseSet<T> data) => _data = data;

        public uint[] Sparse => MemoryUtility.ToArray(_data._sparse, _data._sparseCapacity);
        public T[] Dense => MemoryUtility.ToArray(_data._dense, _data._denseCapacity);
    }
}