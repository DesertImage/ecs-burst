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

        public int Count { get; private set; }

        public void* Values => _dense;

        [NativeDisableUnsafePtrRestriction] internal byte* _dense;
        [NativeDisableUnsafePtrRestriction] internal uint* _sparse;
        [NativeDisableUnsafePtrRestriction] internal uint* _keys;

        internal int _denseCapacity;
        internal int _sparseCapacity;

        private long _elementSize;

        public UnsafeUintUnknownTypeSparseSet(int denseCapacity, int sparseCapacity, long elementSize)
        {
            var uintSize = MemoryUtility.SizeOf<uint>();

            var denseSize = denseCapacity * elementSize;
            _dense = MemoryUtility.AllocateClear<byte>(denseSize);
            _sparse = MemoryUtility.AllocateClear<uint>(sparseCapacity * uintSize);
            _keys = MemoryUtility.AllocateClear<uint>(denseSize);

            _denseCapacity = denseCapacity;
            _sparseCapacity = sparseCapacity;

            Count = 0;

            IsNotNull = true;

            _elementSize = elementSize;
        }

        public void Set<T>(uint key, in T value) where T : unmanaged
        {
// #if DEBUG
//             if (!IsNotNull) throw new NullReferenceException();
// #endif
            if (Contains(key))
            {
                ((T*)_dense)[_sparse[key] - 1] = value;
                return;
            }

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

            var targetIndex = (uint)Count;

            _sparse[key] = targetIndex + 1;
            ((T*)_dense)[targetIndex] = value;
            _keys[targetIndex] = key;

            Count++;

            if (Count <= _denseCapacity - 1) return;

            var newDenseCapacity = _denseCapacity << 1;

            MemoryUtility.Resize(ref _keys, _denseCapacity, newDenseCapacity);

            var oldPtr = _dense;

            var oldSize = _denseCapacity * _elementSize;
            var newSize = newDenseCapacity * _elementSize;

            _dense = (byte*)UnsafeUtility.Malloc(newSize, 0, Allocator.Persistent);

            UnsafeUtility.MemClear(_dense, newSize);
            UnsafeUtility.MemCpy(_dense, oldPtr, oldSize);
            UnsafeUtility.Free(oldPtr, Allocator.Persistent);
            
            MemoryUtility.Resize(ref _keys, _denseCapacity, newDenseCapacity);

            _denseCapacity = newDenseCapacity;
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

        public bool TryGetValue<T>(uint key, out T value) where T : unmanaged
        {
            value = default;

            if (!Contains(key)) return false;

            value = ((T*)_dense)[_sparse[key] - 1];

            return true;
        }

        public readonly ref T Get<T>(uint key) where T : unmanaged => ref ((T*)_dense)[_sparse[key] - 1];
        public void* GetPtr(uint key) => _dense + (_sparse[key] - 1) * _elementSize;
        public readonly T Read<T>(uint key) where T : unmanaged => ((T*)_dense)[_sparse[key] - 1];

        public void Clear()
        {
            Count = 0;

            MemoryUtility.Clear(ref _dense, _denseCapacity * _elementSize);
            MemoryUtility.Clear(ref _keys, _denseCapacity * MemoryUtility.SizeOf<uint>());
            MemoryUtility.Clear(ref _sparse, _sparseCapacity * MemoryUtility.SizeOf<uint>());
        }

        public bool Contains(uint key)
        {
            if (key >= _sparseCapacity) return false;
            return _sparseCapacity > key && _sparse[key] > 0;
        }

        public readonly void Dispose()
        {
            MemoryUtility.Free(_dense);
            MemoryUtility.Free(_sparse);
            MemoryUtility.Free(_keys);
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