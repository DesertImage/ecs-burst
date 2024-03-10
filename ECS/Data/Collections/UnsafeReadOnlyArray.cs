using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.Collections
{
    [DebuggerDisplay("Length = {Length}")]
    [DebuggerTypeProxy(typeof(UnsafeUinTempArrayDebugView<>))]
    public unsafe struct UnsafeReadOnlyArray<T> : IEnumerable<T> where T : unmanaged
    {
        public bool IsNotNull { get; private set; }

        public int Length { get; private set; }

        [NativeDisableUnsafePtrRestriction] internal T* Data;

        public UnsafeReadOnlyArray(T* ptr, int length)
        {
#if DEBUG_MODE
            if (ptr == null) throw new NullReferenceException("ptr is null");
#endif
            Length = length;
            Data = ptr;

            IsNotNull = true;
        }

        public void* GetPtr(int index) => Data + index;
        public void* GetPtr() => Data;

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
            if (!IsNotNull) throw new NullReferenceException("array is null");
            if (Data == null) throw new NullReferenceException("ptr is null");
#endif
            return ref Data[index];
        }

        public struct Enumerator : IEnumerator<T>
        {
            public T Current => _array[_index];
            object IEnumerator.Current => Current;

            private readonly UnsafeReadOnlyArray<T> _array;
            private int _index;

            public Enumerator(UnsafeReadOnlyArray<T> array)
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

    internal sealed class UnsafeUinTempArrayDebugView<T> where T : unmanaged
    {
        private UnsafeReadOnlyArray<T> _data;

        public UnsafeUinTempArrayDebugView(UnsafeReadOnlyArray<T> array) => _data = array;

        public T[] Items => _data.ToArray();
    }
}