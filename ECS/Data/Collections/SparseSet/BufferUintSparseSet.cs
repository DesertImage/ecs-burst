using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DesertImage.ECS;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DesertImage.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(BufferUintSparseSetDebugView<>))]
    public unsafe struct BufferUintSparseSet<T> : IDisposable, IEnumerable<T> where T : unmanaged
    {
        public bool IsNotNull { get; private set; }

        public int Count { get; private set; }

        // public T* Values => _dense;
        public T* Values => _dense;

        // [NativeDisableUnsafePtrRestriction] internal T* _dense;
        // [NativeDisableUnsafePtrRestriction] internal uint* _sparse;
        // [NativeDisableUnsafePtrRestriction] internal uint* _keys;

        internal T* _dense => _densePtr.GetPtr<T>(_allocator);
        internal uint* _sparse => _sparsePtr.GetPtr<uint>(_allocator);
        internal uint* _keys => _keysPtr.GetPtr<uint>(_allocator);

        private Ptr _densePtr;
        private Ptr _sparsePtr;
        private Ptr _keysPtr;

        internal int _denseCapacity;
        internal int _sparseCapacity;

        private MemoryAllocator _allocator;

        public readonly T this[uint index] => _dense[_sparse[index] - 1];

        public BufferUintSparseSet(int denseCapacity, int sparseCapacity, ref MemoryAllocator allocator)
        {
            var uintSize = MemoryUtility.SizeOf<uint>();
            var denseSize = denseCapacity * MemoryUtility.SizeOf<T>();

            _densePtr = allocator.Allocate(denseSize);
            // _dense = _densePtr.GetPtr<T>(in allocator);

            _sparsePtr = allocator.Allocate(sparseCapacity * uintSize);
            // _sparse = (uint*)_sparsePtr.Value;

            _keysPtr = allocator.Allocate(denseCapacity * uintSize);
            // _keys = (uint*)_keysPtr.Value;

            _denseCapacity = denseCapacity;
            _sparseCapacity = sparseCapacity;

            _allocator = allocator;

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

                _allocator.Resize(ref _sparsePtr, newSparseCapacity * MemoryUtility.SizeOf<uint>());
                // _sparse = (uint*)_sparsePtr.Value;

                _sparseCapacity = newSparseCapacity;
            }

            _sparse[key] = targetIndex + 1;
            _dense[targetIndex] = value;
            _keys[targetIndex] = key;

            Count++;

            if (Count < _denseCapacity) return;

            var newDenseCapacity = _denseCapacity << 1;

            _allocator.Resize(ref _densePtr, newDenseCapacity * MemoryUtility.SizeOf<T>());
            // _dense = (T*)_densePtr.Value;

            _allocator.Resize(ref _keysPtr, newDenseCapacity * MemoryUtility.SizeOf<uint>());
            // _keys = (uint*)_keysPtr.Value;

            _denseCapacity = newDenseCapacity;
        }

        public void Remove(uint key)
        {
            var sparseIndex = _sparse[key];

#if DEBUG_MODE
            if (sparseIndex == 0) throw new IndexOutOfRangeException();
#endif
            var denseIndex = sparseIndex - 1;
            var lastIndex = Count - 1;

            if (Count > 1 && denseIndex < lastIndex)
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

        public Ptr GetDensePtr() => _densePtr;
        public Ptr GetSparsePtr() => _sparsePtr;
        public Ptr GetKeysPtr() => _keysPtr;

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
            if (key >= _sparseCapacity) return false;
            return _sparseCapacity > key && _sparse[key] > 0;
        }

        public void Dispose()
        {
#if DEBUG_MODE
            if (!IsNotNull) throw new NullReferenceException("Sparse set is null ");
#endif
            _allocator.Free(_densePtr);
            _allocator.Free(_sparsePtr);
            _allocator.Free(_keysPtr);

            // _dense = null;
            // _sparse = null;
            // _keys = null;

            IsNotNull = false;
            Count = 0;

            _denseCapacity = 0;
            _sparseCapacity = 0;
        }

        public struct Enumerator : IEnumerator<T>
        {
            object IEnumerator.Current => Current;

            public T Current => _sparseSet._dense[_index - 1];

            private readonly BufferUintSparseSet<T> _sparseSet;
            private uint _index;
            private uint _counter;
            private T _current;

            public Enumerator(ref BufferUintSparseSet<T> sparseSet) : this()
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

    internal sealed unsafe class BufferUintSparseSetDebugView<T> where T : unmanaged
    {
        private readonly BufferUintSparseSet<T> _data;

        public BufferUintSparseSetDebugView(BufferUintSparseSet<T> data) => _data = data;

        public uint[] Sparse => MemoryUtility.ToArray(_data._sparse, _data._sparseCapacity);
        public T[] Dense => MemoryUtility.ToArray(_data._dense, _data._denseCapacity);
        public uint[] Keys => MemoryUtility.ToArray(_data._keys, _data._denseCapacity);
    }
}