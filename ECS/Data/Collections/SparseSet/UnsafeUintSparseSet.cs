using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DesertImage.ECS;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(UnsafeUintSparseSetDebugView<>))]
    public unsafe struct UnsafeUintSparseSet<T> : IDisposable, IEnumerable<T> where T : unmanaged
    {
        public bool IsNotNull { get; private set; }

        public int Count { get; private set; }

        public T* Values => _dense;

        [NativeDisableUnsafePtrRestriction] internal T* _dense;
        [NativeDisableUnsafePtrRestriction] internal uint* _sparse;
        [NativeDisableUnsafePtrRestriction] internal uint* _keys;

        internal int _denseCapacity;
        internal int _sparseCapacity;

        public readonly T this[uint index] => _dense[_sparse[index] - 1];

        public UnsafeUintSparseSet(int denseCapacity, int sparseCapacity)
        {
            var uintSize = MemoryUtility.SizeOf<uint>();
            var denseSize = denseCapacity * MemoryUtility.SizeOf<T>();

            _dense = MemoryUtility.AllocateClear<T>(denseSize);
            _sparse = MemoryUtility.AllocateClear<uint>(sparseCapacity * uintSize);
            _keys = MemoryUtility.AllocateClear<uint>(denseSize);

            _denseCapacity = denseCapacity;
            _sparseCapacity = sparseCapacity;

            Count = 0;

            IsNotNull = true;
        }

        public UnsafeUintSparseSet(int capacity)
        {
            var uintSize = MemoryUtility.SizeOf<uint>();
            var denseSize = capacity * MemoryUtility.SizeOf<T>();

            _dense = MemoryUtility.AllocateClear<T>(denseSize);
            _sparse = MemoryUtility.AllocateClear<uint>(capacity * uintSize);
            _keys = MemoryUtility.AllocateClear<uint>(denseSize);

            _denseCapacity = capacity;
            _sparseCapacity = capacity;

            Count = 0;

            IsNotNull = true;
        }

        public UnsafeUintSparseSet(int denseCapacity, int sparseCapacity, int recycledCapacity, T defaultValue)
        {
            var uintSize = MemoryUtility.SizeOf<uint>();
            var denseSize = denseCapacity * MemoryUtility.SizeOf<T>();

            _dense = MemoryUtility.AllocateClear(denseCapacity, defaultValue);
            _sparse = MemoryUtility.AllocateClear<uint>(sparseCapacity * uintSize);
            _keys = MemoryUtility.AllocateClear<uint>(denseSize);

            _denseCapacity = denseCapacity;
            _sparseCapacity = sparseCapacity;

            Count = 0;

            IsNotNull = true;
        }

        public void Set(uint key, in T value)
        {
            if (Contains(key))
            {
                _dense[_sparse[key] - 1] = value;
                return;
            }

            var targetIndex = (uint)Count;

            if (key >= _sparseCapacity)
            {
                var newSparseCapacity = _sparseCapacity << 1;
                if (newSparseCapacity <= key)
                {
                    newSparseCapacity = (int)(key + 1);
                }
                
                MemoryUtility.Resize(ref _sparse, _sparseCapacity, newSparseCapacity);
                _sparseCapacity = newSparseCapacity;
            }

            _sparse[key] = targetIndex + 1;
            _dense[targetIndex] = value;
            _keys[targetIndex] = key;

            Count++;

            if (Count >= _denseCapacity)
            {
                var newDenseCapacity = _denseCapacity << 1;
                MemoryUtility.Resize(ref _dense, _denseCapacity, newDenseCapacity);
                MemoryUtility.Resize(ref _keys, _denseCapacity, newDenseCapacity);
                _denseCapacity = newDenseCapacity;
            }
        }

        public void Remove(uint key)
        {
            var sparseIndex = _sparse[key];

#if DEBUG_MODE
            if (sparseIndex == 0) throw new IndexOutOfRangeException();
#endif
            var denseIndex = sparseIndex - 1;

            if (Count > 1)
            {
                var lastIndex = Count - 1;

                _dense[denseIndex] = _dense[lastIndex];
                _sparse[_keys[denseIndex]] = denseIndex + 1;
                _keys[denseIndex] = _keys[lastIndex];
            }
            else
            {
                _dense[denseIndex] = default;
                _keys[denseIndex] = default;
            }
            
            _sparse[key] = 0;

            Count--;
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

        public void Clear()
        {
            Count = 0;

            for (var i = 0; i < _denseCapacity; i++)
            {
                _dense[i] = default;
                _keys[i] = default;
            }

            for (var i = 0; i < _sparseCapacity; i++)
            {
                _sparse[i] = 0;
            }
        }

        public bool Contains(uint key)
        {
            if(key >= _sparseCapacity) return false;
            return _sparseCapacity > key && _sparse[key] > 0;
        }

        public readonly void Dispose()
        {
            MemoryUtility.Free(_dense);
            MemoryUtility.Free(_sparse);
            MemoryUtility.Free(_keys);
        }

        public struct Enumerator : IEnumerator<T>
        {
            object IEnumerator.Current => Current;

            public T Current => _sparseSet._dense[_index - 1];

            private readonly UnsafeUintSparseSet<T> _sparseSet;
            private uint _index;
            private uint _counter;
            private T _current;

            public Enumerator(ref UnsafeUintSparseSet<T> sparseSet) : this()
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

    internal sealed unsafe class UnsafeUintSparseSetDebugView<T> where T : unmanaged
    {
        private readonly UnsafeUintSparseSet<T> _data;

        public UnsafeUintSparseSetDebugView(UnsafeUintSparseSet<T> data) => _data = data;

        public uint[] Sparse => MemoryUtility.ToArray(_data._sparse, _data._sparseCapacity);
        public T[] Dense => MemoryUtility.ToArray(_data._dense, _data._denseCapacity);
    }
}