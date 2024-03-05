using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DesertImage.ECS;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.Collections
{
    [DebuggerDisplay("Length = {Length}")]
    [DebuggerTypeProxy(typeof(UnsafeArrayDebugView<>))]
    public unsafe struct UnsafeArray<T> : IDisposable, IEnumerable<T> where T : unmanaged
    {
        public bool IsNotNull { get; private set; }

        public int Length { get; private set; }

        [NativeDisableUnsafePtrRestriction] internal T* Data;

        private readonly long _elementSize;
        private readonly Allocator _allocator;

        public UnsafeArray(int length, Allocator allocator) : this()
        {
            Length = length;

            _elementSize = UnsafeUtility.SizeOf<T>();
            Data = (T*)UnsafeUtility.Malloc(length * _elementSize, 0, allocator);

            _allocator = allocator;

            IsNotNull = true;
        }

        public UnsafeArray(int length, bool clearMemory, Allocator allocator) : this()
        {
            Length = length;

            _elementSize = UnsafeUtility.SizeOf<T>();
            var fullSize = length * _elementSize;

            Data = (T*)UnsafeUtility.Malloc(fullSize, 0, allocator);
            _allocator = allocator;

            if (!clearMemory) return;

            UnsafeUtility.MemClear(Data, fullSize);

            IsNotNull = true;
        }

        public UnsafeArray(int length, Allocator allocator, T defaultValue) : this()
        {
            Length = length;

            _elementSize = UnsafeUtility.SizeOf<T>();
            Data = (T*)UnsafeUtility.Malloc(length * _elementSize, UnsafeUtility.AlignOf<T>(), allocator);
            _allocator = allocator;

            for (var i = 0; i < length; i++)
            {
                Data[i] = defaultValue;
            }

            IsNotNull = true;
        }

        public UnsafeArray(T* ptr, int length, Allocator allocator)
        {
            Length = length;

            Data = ptr;

            _elementSize = MemoryUtility.SizeOf<T>();
            _allocator = allocator;

            IsNotNull = true;
        }

        public UnsafeArray<T> Resize(int length, bool clear = true)
        {
#if DEBUG_MODE
            if (length < Length) throw new Exception("new length is less then original");
#endif
            var oldData = Data;
            var fullSize = length * _elementSize;
            Data = (T*)UnsafeUtility.Malloc(fullSize, 0, _allocator);

            if (clear)
            {
                UnsafeUtility.MemClear(Data, fullSize);
            }

            UnsafeUtility.MemCpy(Data, oldData, Length * _elementSize);
            UnsafeUtility.Free(oldData, _allocator);

            Length = length;

            return this;
        }

        public void Clear()
        {
            UnsafeUtility.MemClear(Data, Length * UnsafeUtility.SizeOf(typeof(T)));
        }

        public void CopyTo(UnsafeArray<T> target)
        {
            UnsafeUtility.MemCpy(target.Data, Data, Length * _elementSize);
        }

        public readonly void Dispose()
        {
#if DEBUG_MODE
            if (Data == null) throw new NullReferenceException();
#endif
            UnsafeUtility.Free(Data, _allocator);
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

        public readonly ref T Get(int index) => ref Data[index];

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