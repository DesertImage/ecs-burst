using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public unsafe struct UnsafeArray<T> : IDisposable, IEnumerable<T> where T : unmanaged
    {
        public bool IsNull => Data == null;

        public int Length { get; private set; }

        internal void* Data;

        private readonly long _elementSize;
        private readonly Allocator _allocator;

        public UnsafeArray(int length, Allocator allocator) : this()
        {
            Length = length;

            _elementSize = UnsafeUtility.SizeOf<T>();
            Data = UnsafeUtility.Malloc(length * _elementSize, 0, allocator);
            _allocator = allocator;
        }
        
        public UnsafeArray(int length, Allocator allocator, T defaultValue) : this()
        {
            Length = length;

            _elementSize = UnsafeUtility.SizeOf<T>();
            Data = UnsafeUtility.Malloc(length * _elementSize, 0, allocator);
            _allocator = allocator;
            
            for (var i = 0; i < length; i++)
            {
                UnsafeUtility.CopyStructureToPtr(ref defaultValue, GetIndexPointer(i));
            }
        }

        public void Dispose()
        {
            UnsafeUtility.Free(Data, _allocator);
            Data = null;
        }

        public UnsafeArray<T> Resize(int length)
        {
#if DEBUG
            if (length < Length) throw new Exception("new length is less then original");
#endif
            var oldData = Data;
            Data = (byte*)UnsafeUtility.Malloc(length * _elementSize, 0, _allocator);

            UnsafeUtility.MemCpy(Data, oldData, Length * _elementSize);
            UnsafeUtility.Free(oldData, _allocator);

            Length = length;

            return this;
        }

        public UnsafeArray<T> ResizeToNew(int newSize)
        {
            var target = new UnsafeArray<T>(newSize, _allocator);
            CopyTo(target);
            return target;
        }

        public void CopyTo(UnsafeArray<T> target)
        {
            UnsafeUtility.MemCpy(target.Data, Data, Length * _elementSize);
        }

        private readonly void* GetIndexPointer(int index) => (void*)((IntPtr)Data + (int)(_elementSize * index));

        public T this[int index]
        {
            get => *(T*)GetIndexPointer(index);
            set => UnsafeUtility.CopyStructureToPtr(ref value, GetIndexPointer(index));
        }

        public readonly void Set(int index, T value) => UnsafeUtility.CopyStructureToPtr(ref value, GetIndexPointer(index));

        public readonly ref T Get(int index) => ref *(T*)GetIndexPointer(index);

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
}