using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DesertImage.ECS;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(UnsafeUshortSparseSetDebugView<>))]
    public unsafe struct UnsafeUshortSparseSet<T> : IDisposable, IEnumerable<T> where T : unmanaged
    {
        public bool IsNotNull { get; private set; }

        public int Count { get; private set; }

        public T* Values => _dense;

        [NativeDisableUnsafePtrRestriction] internal T* _dense;
        [NativeDisableUnsafePtrRestriction] internal ushort* _sparse;

        internal int _denseCapacity;
        internal int _sparseCapacity;

        public readonly T this[ushort index] => _dense[_sparse[index] - 1];

        public UnsafeUshortSparseSet(int denseCapacity, int sparseCapacity, int recycledCapacity = 100)
        {
            var ushortSize = MemoryUtility.SizeOf<ushort>();

            _dense = MemoryUtility.AllocateClear<T>(denseCapacity * MemoryUtility.SizeOf<T>());
            _sparse = MemoryUtility.AllocateClear<ushort>(sparseCapacity * ushortSize);
            MemoryUtility.AllocateClear<ushort>(recycledCapacity * ushortSize);

            _denseCapacity = denseCapacity;
            _sparseCapacity = sparseCapacity;

            Count = 0;

            IsNotNull = true;
        }

        public UnsafeUshortSparseSet(int capacity)
        {
            var ushortSize = MemoryUtility.SizeOf<ushort>();

            _dense = MemoryUtility.AllocateClear<T>(capacity * MemoryUtility.SizeOf<T>());
            _sparse = MemoryUtility.AllocateClear<ushort>(capacity * ushortSize);
            MemoryUtility.AllocateClear<ushort>(capacity * ushortSize);

            _denseCapacity = capacity;
            _sparseCapacity = capacity;

            Count = 0;

            IsNotNull = true;
        }

        public UnsafeUshortSparseSet(int denseCapacity, int sparseCapacity, int recycledCapacity, T defaultValue)
        {
            var ushortSize = MemoryUtility.SizeOf<ushort>();

            _dense = MemoryUtility.AllocateClear(denseCapacity * MemoryUtility.SizeOf<T>(), defaultValue);
            _sparse = MemoryUtility.AllocateClear<ushort>(sparseCapacity * ushortSize);
            MemoryUtility.AllocateClear<ushort>(recycledCapacity * ushortSize);

            _denseCapacity = denseCapacity;
            _sparseCapacity = sparseCapacity;

            Count = 0;

            IsNotNull = true;
        }

        public void Set(ushort key, in T value)
        {
            if (Contains(key))
            {
                _dense[_sparse[key] - 1] = value;
                return;
            }

            var targetIndex = (ushort)Count;

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
            var sparseIndex = _sparse[key];

#if DEBUG
            if (sparseIndex == 0) throw new IndexOutOfRangeException();
#endif
            if (Count > 1)
            {
                _dense[sparseIndex - 1] = _dense[Count - 1];
                _sparse[Count - 1] = sparseIndex;
            }
            else
            {
                _dense[sparseIndex - 1] = default;
            }
            
            _sparse[key] = 0;

            Count--;
        }

        public bool TryGetValue(ushort key, out T value)
        {
            value = default;

            if (!Contains(key)) return false;

            value = _dense[_sparse[key] - 1];

            return true;
        }

        public readonly T Read(ushort key) => _dense[_sparse[key] - 1];
        public readonly ref T Get(ushort key) => ref _dense[_sparse[key] - 1];

        public void Clear()
        {
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

        public bool Contains(ushort key) => _sparseCapacity > key && _sparse[key] > 0;

        public readonly void Dispose()
        {
            MemoryUtility.Free(_dense);
            MemoryUtility.Free(_sparse);
        }

        public struct Enumerator : IEnumerator<T>
        {
            object IEnumerator.Current => Current;

            public T Current => _sparseSet._dense[_index - 1];

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

    internal sealed unsafe class UnsafeUshortSparseSetDebugView<T> where T : unmanaged
    {
        private readonly UnsafeUshortSparseSet<T> _data;

        public UnsafeUshortSparseSetDebugView(UnsafeUshortSparseSet<T> data) => _data = data;

        public ushort[] Sparse => MemoryUtility.ToArray(_data._sparse, _data._sparseCapacity);
        public T[] Dense => MemoryUtility.ToArray(_data._dense, _data._denseCapacity);
    }
}