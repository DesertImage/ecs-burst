using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DesertImage.ECS;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.Collections
{
    [DebuggerDisplay("_length = {Length}")]
    [DebuggerTypeProxy(typeof(UnsafeArrayDebugView<>))]
    public unsafe struct UnsafeArray<T> : IDisposable, IEnumerable<T> where T : unmanaged
    {
        public bool IsNotNull => _isNotNull;
        private bool _isNotNull;

        public int Length => _length;
        private int _length;

        [NativeDisableUnsafePtrRestriction] internal T* Data;

        private readonly long _elementSize;
        private readonly Allocator _allocator;

        public UnsafeArray(int length, Allocator allocator) : this()
        {
            _length = length;

            _elementSize = UnsafeUtility.SizeOf<T>();
            Data = (T*)UnsafeUtility.Malloc(length * _elementSize, 0, allocator);

            _allocator = allocator;

            _isNotNull = true;
        }

        public UnsafeArray(int length, bool clearMemory, Allocator allocator) : this()
        {
            _length = length;

            _elementSize = UnsafeUtility.SizeOf<T>();
            var fullSize = length * _elementSize;

            Data = (T*)UnsafeUtility.Malloc(fullSize, 0, allocator);
            _allocator = allocator;

            if (!clearMemory) return;

            UnsafeUtility.MemClear(Data, fullSize);

            _isNotNull = true;
        }

        public UnsafeArray(int length, Allocator allocator, T defaultValue) : this()
        {
            _length = length;

            _elementSize = UnsafeUtility.SizeOf<T>();
            Data = (T*)UnsafeUtility.Malloc(length * _elementSize, UnsafeUtility.AlignOf<T>(), allocator);
            _allocator = allocator;

            for (var i = 0; i < length; i++)
            {
                Data[i] = defaultValue;
            }

            _isNotNull = true;
        }

        public UnsafeArray(T* ptr, int length, Allocator allocator)
        {
#if DEBUG_MODE
            if (ptr == null) throw new NullReferenceException("ptr is null");
#endif
            _length = length;

            Data = ptr;

            _elementSize = MemoryUtility.SizeOf<T>();
            _allocator = allocator;

            _isNotNull = true;
        }

        public UnsafeArray<T> Resize(int length, bool clear = true)
        {
#if DEBUG_MODE
            if (length < Length) throw new Exception("new length is less then original");
#endif
            var oldData = Data;
            var fullSize = length * _elementSize;
            Data = MemoryUtility.Allocate<T>(fullSize, _allocator);

            if (clear)
            {
                MemoryUtility.Clear(Data, fullSize);
            }

            MemoryUtility.Copy(Data, oldData, Length * _elementSize);
            MemoryUtility.Free(oldData, _allocator);

            _length = length;

            return this;
        }

        public void* GetPtr(int index) => Data + index;
        public void* GetPtr() => Data;

        public void Clear()
        {
            MemoryUtility.Clear(Data, Length * MemoryUtility.SizeOf<T>());
        }

        public void CopyTo(UnsafeArray<T> target)
        {
            MemoryUtility.Copy(target.Data, Data, Length * _elementSize);
        }

        public void Dispose()
        {
#if DEBUG_MODE
            if (!IsNotNull) throw new NullReferenceException();
            if (Data == null) throw new NullReferenceException();
#endif
            _isNotNull = false;
            _length = 0;
            MemoryUtility.Free(Data, _allocator);
        }

        public T[] ToArray()
        {
            var array = new T[Length];

            for (var i = 0; i < Length; i++)
            {
                array[i] = this[i];
            }

            return array;
        }

        public T this[int index]
        {
            get => Data[index];
            set => Data[index] = value;
        }

        public readonly ref T Get(int index)
        {
#if DEBUG_MODE
            if (!_isNotNull) throw new NullReferenceException("array is null");
            if (Data == null) throw new NullReferenceException("ptr is null");
#endif
            return ref Data[index];
        }

        public struct Enumerator : IEnumerator<T>
        {
            public T Current => _array[_index];
            object IEnumerator.Current => Current;

            private readonly UnsafeArray<T> _array;
            private int _index;

            public Enumerator(UnsafeArray<T> array)
            {
                _array = array;
                _index = -1;
            }

            public bool MoveNext()
            {
                ++_index;
                return _index < _array.Length;
            }

            public void Reset() => _index = -1;

            public void Dispose()
            {
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }

    internal sealed class UnsafeArrayDebugView<T> where T : unmanaged
    {
        private UnsafeArray<T> _data;

        public UnsafeArrayDebugView(UnsafeArray<T> array) => _data = array;

        public T[] Items => _data.ToArray();
    }
}