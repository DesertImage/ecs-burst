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
    [DebuggerTypeProxy(typeof(UnsafeUintUnknownTypeSparseSetDebugView))]
    public unsafe struct UnsafeUintUnknownTypeSparseSet : IDisposable, IEnumerable<IntPtr>
    {
        public bool IsNotNull { get; private set; }

        public int Count => *_count;

        public void* Values => _dense;

        [NativeDisableUnsafePtrRestriction] internal byte* _dense;
        [NativeDisableUnsafePtrRestriction] internal uint* _sparse;
        [NativeDisableUnsafePtrRestriction] internal uint* _keys;

        [NativeDisableUnsafePtrRestriction] private int* _count;

        internal int _denseCapacity;
        internal int _sparseCapacity;

        private long _elementSize;

        public UnsafeUintUnknownTypeSparseSet(int denseCapacity, int sparseCapacity, long elementSize)
        {
            var uintSize = MemoryUtility.SizeOf<uint>();

            _dense = MemoryUtility.AllocateClear<byte>(denseCapacity * elementSize);
            _sparse = MemoryUtility.AllocateClear<uint>(sparseCapacity * uintSize);
            _keys = MemoryUtility.AllocateClear<uint>(denseCapacity * uintSize);
            _count = MemoryUtility.AllocateInstance(0);

            _denseCapacity = denseCapacity;
            _sparseCapacity = sparseCapacity;

            IsNotNull = true;

            _elementSize = elementSize;
        }

        public void AddOrSet<T>(uint key, in T value) where T : unmanaged
        {
#if DEBUG_MODE
            if (!IsNotNull) throw new NullReferenceException();
#endif
            if (Contains(key))
            {
                Update(key, value);
                return;
            }

            Add(key, value);
        }

        public void Add<T>(uint key, in T value) where T : unmanaged
        {
#if DEBUG_MODE
            if (!IsNotNull) throw new NullReferenceException();
#endif
            if (key >= _sparseCapacity)
            {
                var newSparseCapacity = _sparseCapacity << 1;
                if (newSparseCapacity <= key)
                {
                    newSparseCapacity = (int)(key + 1);
                }

                _sparse = MemoryUtility.Resize(_sparse, _sparseCapacity, newSparseCapacity);
                _sparseCapacity = newSparseCapacity;
            }

            var targetIndex = (uint)Count;

            _sparse[key] = targetIndex + 1;
            ((T*)_dense)[targetIndex] = value;
            _keys[targetIndex] = key;

            (*_count)++;

            if (Count <= _denseCapacity - 1) return;

            var newDenseCapacity = _denseCapacity << 1;

            var oldPtr = _dense;

            var oldSize = _denseCapacity * _elementSize;
            var newSize = newDenseCapacity * _elementSize;
            var uintSize = MemoryUtility.SizeOf<uint>();

            _keys = MemoryUtility.Resize(_keys, _denseCapacity * uintSize, newDenseCapacity * uintSize);
            _dense = (byte*)UnsafeUtility.Malloc(newSize, 0, Allocator.Persistent);

            MemoryUtility.Clear(_dense, newSize);
            MemoryUtility.Copy(_dense, oldPtr, oldSize);
            MemoryUtility.Free(oldPtr);

            _denseCapacity = newDenseCapacity;
        }

        public void Update<T>(uint key, in T value) where T : unmanaged
        {
#if DEBUG_MODE
            if (!IsNotNull) throw new NullReferenceException();
#endif
            ((T*)_dense)[_sparse[key] - 1] = value;
        }

        public void Remove(uint key)
        {
            var sparseIndex = _sparse[key];

#if DEBUG_MODE
            if (*_count == 0) throw new IndexOutOfRangeException();
            if(!IsNotNull) throw new NullReferenceException();
            if (sparseIndex == 0) throw new IndexOutOfRangeException();
#endif
            var denseIndex = sparseIndex - 1;
            var lastIndex = Count - 1;

            var densePtrIndex = denseIndex * _elementSize;
            var lastPtrIndex = lastIndex * _elementSize;

            if (Count > 1 && denseIndex < lastIndex)
            {
                for (var i = 0; i < _elementSize; i++)
                {
                    _dense[densePtrIndex + i] = _dense[lastPtrIndex + i];
                }

                _sparse[_keys[lastIndex]] = denseIndex + 1;
                _keys[denseIndex] = _keys[lastIndex];
            }
            else
            {
                for (var i = 0; i < _elementSize; i++)
                {
                    _dense[densePtrIndex + i] = default;
                }

                _keys[denseIndex] = default;
            }

            _sparse[key] = 0;

            (*_count)--;
        }

        public bool TryGetValue<T>(uint key, out T value) where T : unmanaged
        {
            value = default;

            if (!Contains(key)) return false;

            value = ((T*)_dense)[_sparse[key] - 1];

            return true;
        }

        public ref T Get<T>(uint key) where T : unmanaged
        {
#if DEBUG_MODE
            if (!Contains(key)) throw new Exception($"Sparse {typeof(T)} not contains key {key}");
#endif
            return ref ((T*)_dense)[_sparse[key] - 1];
        }

        public void* GetPtr(uint key)
        {
#if DEBUG_MODE
            if (!Contains(key)) throw new Exception($"Sparse set doesn't contains key: {key}");
#endif
            var denseIndex = _sparse[key] - 1;
            return _dense + denseIndex * _elementSize;
        }

        public void* GetPtr() => _dense;

        public T Read<T>(uint key) where T : unmanaged
        {
#if DEBUG_MODE
            if (!Contains(key)) throw new Exception($"Sparse {typeof(T)} not contains key {key}");
#endif
            return ((T*)_dense)[_sparse[key] - 1];
        }

        public void Clear()
        {
            *_count = 0;

            var intSize = MemoryUtility.SizeOf<uint>();

            MemoryUtility.Clear(_dense, _denseCapacity * _elementSize);
            MemoryUtility.Clear(_keys, _denseCapacity * intSize);
            MemoryUtility.Clear(_sparse, _sparseCapacity * intSize);
        }

        public bool Contains(uint key)
        {
            return _sparseCapacity > key && _sparse[key] > 0;
        }

        public UnsafeUintReadOnlySparseSet<T> ToReadOnly<T>() where T : unmanaged
        {
            return new UnsafeUintReadOnlySparseSet<T>
            (
                (T*)_dense,
                _denseCapacity,
                _sparse,
                _sparseCapacity,
                _keys,
                _count
            );
        }

        public void Dispose()
        {
#if DEBUG_MODE
            if (!IsNotNull) throw new NullReferenceException("Sparse set is null");
#endif
            MemoryUtility.Free(_dense);
            MemoryUtility.Free(_sparse);
            MemoryUtility.Free(_keys);
            MemoryUtility.Free(_count);

            _dense = null;
            _sparse = null;
            _keys = null;
            _count = null;
        }

        public struct Enumerator : IEnumerator<IntPtr>
        {
            object IEnumerator.Current => Current;

            public IntPtr Current => (IntPtr)(_sparseSet._dense + (_index - 1) * _sparseSet._elementSize);

            private readonly UnsafeUintUnknownTypeSparseSet _sparseSet;
            private uint _index;
            private uint _counter;
            private IntPtr _current;

            public Enumerator(ref UnsafeUintUnknownTypeSparseSet sparseSet) : this()
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

        IEnumerator<IntPtr> IEnumerable<IntPtr>.GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }

    internal sealed unsafe class UnsafeUintUnknownTypeSparseSetDebugView
    {
        private readonly UnsafeUintUnknownTypeSparseSet _data;

        public UnsafeUintUnknownTypeSparseSetDebugView(UnsafeUintUnknownTypeSparseSet data) => _data = data;

        public uint[] Sparse => MemoryUtility.ToArray(_data._sparse, _data._sparseCapacity);
        public uint[] Keys => MemoryUtility.ToArray(_data._keys, _data._denseCapacity);
    }
}