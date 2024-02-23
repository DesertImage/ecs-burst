using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DesertImage.ECS;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(UnsafeSparseSetDebugView<>))]
    public unsafe struct UnsafeSparseSet<T> : IDisposable, IEnumerable<T> where T : unmanaged
    {
        public bool IsNotNull { get; private set; }

        public int Count { get; private set; }

        public T* Values => _dense;

        [NativeDisableUnsafePtrRestriction] internal T* _dense;
        [NativeDisableUnsafePtrRestriction] internal int* _sparse;

        internal int _denseCapacity;
        internal int _sparseCapacity;

        public readonly T this[int index] => _dense[_sparse[index]];

        public UnsafeSparseSet(int denseCapacity, int sparseCapacity, int recycledCapacity = 100)
        {
            var intSize = MemoryUtility.SizeOf<int>();

            _dense = MemoryUtility.AllocateClear<T>(denseCapacity * MemoryUtility.SizeOf<T>());
            _sparse = MemoryUtility.AllocateClear(sparseCapacity * intSize, -1);

            _denseCapacity = denseCapacity;
            _sparseCapacity = sparseCapacity;

            Count = 0;

            IsNotNull = true;
        }

        public UnsafeSparseSet(int capacity)
        {
            var intSize = MemoryUtility.SizeOf<int>();

            _dense = MemoryUtility.AllocateClear<T>(capacity * MemoryUtility.SizeOf<T>());
            _sparse = MemoryUtility.AllocateClear(capacity * intSize, -1);

            _denseCapacity = capacity;
            _sparseCapacity = capacity;

            Count = 0;

            IsNotNull = true;
        }

        public UnsafeSparseSet(int denseCapacity, int sparseCapacity, int recycledCapacity, T defaultValue)
        {
            var intSize = MemoryUtility.SizeOf<int>();

            _dense = MemoryUtility.AllocateClear(denseCapacity * MemoryUtility.SizeOf<T>(), defaultValue);
            _sparse = MemoryUtility.AllocateClear(sparseCapacity * intSize, -1);

            _denseCapacity = denseCapacity;
            _sparseCapacity = sparseCapacity;

            Count = 0;

            IsNotNull = true;
        }

        public void Set(int key, in T value)
        {
            if (Contains(key))
            {
                _dense[_sparse[key]] = value;
                return;
            }

            var targetIndex = Count;

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
            var sparseIndex = _sparse[key];
            
            _dense[sparseIndex] = _dense[Count - 1];
            _sparse[Count - 1] = sparseIndex;
            _sparse[key] = -1;

            Count--;
        }

        public readonly ref readonly T Get(int key) => ref _dense[_sparse[key]];
        public readonly T Read(uint key) => _dense[_sparse[key]];

        public void Clear()
        {
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

        public bool Contains(int key) => _sparseCapacity > key && _sparse[key] != -1;

        public readonly void Dispose()
        {
            MemoryUtility.Free(_dense);
            MemoryUtility.Free(_sparse);
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