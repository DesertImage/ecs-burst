using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DesertImage.ECS;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.Collections
{
    [DebuggerDisplay("_count = {_count}")]
    [DebuggerTypeProxy(typeof(UnsafeUintSparseSetDebugView<>))]
    public unsafe struct UnsafeUintSparseSet<T> : IDisposable where T : unmanaged
    {
        public bool IsNotNull => _isNotNull;
        private bool _isNotNull;

        public int Count => _count;
        private int _count;

        public T* Values => _dense;

        [NativeDisableUnsafePtrRestriction] internal T* _dense;
        [NativeDisableUnsafePtrRestriction] internal uint* _sparse;
        [NativeDisableUnsafePtrRestriction] internal uint* _keys;

        internal int _denseCapacity;
        internal int _sparseCapacity;

        private Allocator _allocator;

        public readonly T this[uint index] => _dense[_sparse[index] - 1];

        public UnsafeUintSparseSet(int denseCapacity, int sparseCapacity, Allocator allocator = Allocator.Persistent)
        {
            var uintSize = MemoryUtility.SizeOf<uint>();
            var denseSize = denseCapacity * MemoryUtility.SizeOf<T>();

            _dense = MemoryUtility.AllocateClear<T>(denseSize, allocator);
            _sparse = MemoryUtility.AllocateClear<uint>(sparseCapacity * uintSize, allocator);
            _keys = MemoryUtility.AllocateClear<uint>(denseCapacity * uintSize, allocator);

            _denseCapacity = denseCapacity;
            _sparseCapacity = sparseCapacity;

            _count = 0;

            _isNotNull = true;

            _allocator = allocator;
        }

        public UnsafeUintSparseSet(int capacity, Allocator allocator = Allocator.Persistent)
        {
            var uintSize = MemoryUtility.SizeOf<uint>();
            var fullUintSize = capacity * uintSize;
            var denseSize = capacity * MemoryUtility.SizeOf<T>();

            _dense = MemoryUtility.AllocateClear<T>(denseSize, allocator);
            _sparse = MemoryUtility.AllocateClear<uint>(fullUintSize, allocator);
            _keys = MemoryUtility.AllocateClear<uint>(fullUintSize, allocator);

            _denseCapacity = capacity;
            _sparseCapacity = capacity;

            _count = 0;

            _isNotNull = true;

            _allocator = allocator;
        }

        public void Add(uint key, in T value)
        {
#if DEBUG_MODE
            if (_count >= _denseCapacity) throw new IndexOutOfRangeException();
#endif
            var targetIndex = (uint)_count;

            if (key >= _sparseCapacity)
            {
                var newSparseCapacity = _sparseCapacity << 1;
                if (newSparseCapacity <= key)
                {
                    newSparseCapacity = (int)(key + 1);
                }

                _sparse = MemoryUtility.Resize(_sparse, _sparseCapacity, newSparseCapacity, _allocator);
                _sparseCapacity = newSparseCapacity;
            }

            _sparse[key] = targetIndex + 1;
            _dense[targetIndex] = value;
            _keys[targetIndex] = key;

            _count++;

            if (_count >= _denseCapacity)
            {
                var newDenseCapacity = _denseCapacity << 1;
                _dense = MemoryUtility.Resize(_dense, _denseCapacity, newDenseCapacity, _allocator);
                _keys = MemoryUtility.Resize(_keys, _denseCapacity, newDenseCapacity, _allocator);
                _denseCapacity = newDenseCapacity;
            }
        }
        
        public void AddOrUpdate(uint key, in T value)
        {
            if (Contains(key))
            {
                _dense[_sparse[key] - 1] = value;
                return;
            }

#if DEBUG_MODE
            if (_count >= _denseCapacity) throw new IndexOutOfRangeException();
#endif
            var targetIndex = (uint)_count;

            if (key >= _sparseCapacity)
            {
                var newSparseCapacity = _sparseCapacity << 1;
                if (newSparseCapacity <= key)
                {
                    newSparseCapacity = (int)(key + 1);
                }

                _sparse = MemoryUtility.Resize(_sparse, _sparseCapacity, newSparseCapacity, _allocator);
                _sparseCapacity = newSparseCapacity;
            }

            _sparse[key] = targetIndex + 1;
            _dense[targetIndex] = value;
            _keys[targetIndex] = key;

            _count++;

            if (_count >= _denseCapacity)
            {
                var newDenseCapacity = _denseCapacity << 1;
                _dense = MemoryUtility.Resize(_dense, _denseCapacity, newDenseCapacity, _allocator);
                _keys = MemoryUtility.Resize(_keys, _denseCapacity, newDenseCapacity, _allocator);
                _denseCapacity = newDenseCapacity;
            }
        }

        public void Update(uint key, in T value)
        {
            _dense[_sparse[key] - 1] = value;
        }

        public void Remove(uint key)
        {
            var sparseIndex = _sparse[key];
#if DEBUG_MODE
            if (sparseIndex == 0) throw new IndexOutOfRangeException($"Key: {key}");
#endif
            var denseIndex = sparseIndex - 1;
            var lastIndex = _count - 1;

            if (_count > 1 && denseIndex < lastIndex)
            {
                _dense[denseIndex] = _dense[lastIndex];

                var lastKey = _keys[lastIndex];
                _sparse[lastKey] = denseIndex + 1;
                _keys[denseIndex] = lastKey;
            }
            else
            {
                _dense[denseIndex] = default;
                _keys[denseIndex] = default;
            }

            _sparse[key] = 0;

            _count--;
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
        public readonly uint ReadInvert(int key) => _sparse[_keys[key]];

        public void Clear()
        {
            _count = 0;

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
#if DEBUG_MODE
            if (!IsNotNull) throw new NullReferenceException("Sparse set is null");
#endif
            if (_count == 0) return false;
            if (key >= _sparseCapacity) return false;
            return _sparseCapacity > key && _sparse[key] > 0;
        }

        public UnsafeArray<T> ToUnsafeArray(Allocator allocator)
        {
            var array = new UnsafeArray<T>(_count, allocator);

            for (var i = 0; i < _count; i++)
            {
                array[i] = _dense[i];
            }

            return array;
        }

        public void Dispose()
        {
#if DEBUG_MODE
            if (!IsNotNull) throw new NullReferenceException("Sparse set is null ");
            if (_dense == null) throw new NullReferenceException("Sparse set is null ");
            if (_sparse == null) throw new NullReferenceException("Sparse set is null ");
            if (_keys == null) throw new NullReferenceException("Sparse set is null ");
#endif
            MemoryUtility.Free(_dense, _allocator);
            MemoryUtility.Free(_sparse, _allocator);
            MemoryUtility.Free(_keys, _allocator);

            _dense = null;
            _sparse = null;
            _keys = null;

            _isNotNull = false;
            _count = 0;

            _denseCapacity = 0;
            _sparseCapacity = 0;
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
                return _index - 1 < _sparseSet._count;
            }

            public void Reset() => _index = 0;

            public void Dispose()
            {
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(ref this);
    }

    internal sealed unsafe class UnsafeUintSparseSetDebugView<T> where T : unmanaged
    {
        private readonly UnsafeUintSparseSet<T> _data;

        public UnsafeUintSparseSetDebugView(UnsafeUintSparseSet<T> data) => _data = data;

        public uint[] Sparse => MemoryUtility.ToArray(_data._sparse, _data._sparseCapacity);
        public T[] Dense => MemoryUtility.ToArray(_data._dense, _data._denseCapacity);
        public uint[] Keys => MemoryUtility.ToArray(_data._keys, _data._denseCapacity);
    }
}