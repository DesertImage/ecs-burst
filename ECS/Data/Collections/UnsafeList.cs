using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DesertImage.ECS;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Debug = UnityEngine.Debug;

namespace DesertImage.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(UnsafeListDebugView<>))]
    public unsafe struct UnsafeList<T> : IDisposable, IEnumerable<T> where T : unmanaged, IEquatable<T>
    {
        public bool IsNotNull { get; private set; }

        public int Count { get; private set; }

        [NativeDisableUnsafePtrRestriction] private T* _data;
        private int _capacity;
        private long _size;
        private readonly Allocator _allocator;

        public UnsafeList(int capacity, Allocator allocator)
        {
            _size = capacity * UnsafeUtility.SizeOf<T>();

            _data = MemoryUtility.AllocateClear<T>(_size, allocator);
            _capacity = capacity;

            IsNotNull = true;
            Count = 0;
            _allocator = allocator;
        }

        public UnsafeList(int capacity, Allocator allocator, T defaultValue) : this()
        {
            _data = MemoryUtility.AllocateClear(capacity * UnsafeUtility.SizeOf<T>(), defaultValue, allocator);
            _capacity = capacity;
        }

        public void Add(T element)
        {
#if DEBUG_MODE
            if (Contains(element)) Debug.LogWarning($"List already contains {element}");
#endif
            if (Count >= _capacity)
            {
                var oldCapacity = _capacity;
                _capacity <<= 1;
                _size = _capacity * UnsafeUtility.SizeOf<T>();
                _data = MemoryUtility.Resize(_data, oldCapacity, _capacity, _allocator);
            }

            _data[Count] = element;

            Count++;
        }

        public void Remove(T element)
        {
            if (!Contains(element))
            {
#if DEBUG_MODE
                throw new Exception($"not contains {element}");
#else
                return;
#endif
            }

            RemoveAt(IndexOf(element));
        }

        public void RemoveAt(int index)
        {
            _data[index] = default;
            MemoryUtility.ShiftLeft(ref _data, index, _capacity);
            Count--;
        }

        public void Clear()
        {
            MemoryUtility.Clear(_data, _capacity);
            Count = 0;
        }

        public bool Contains(T element)
        {
            for (var i = 0; i < _capacity; i++)
            {
                if (element.Equals(_data[i])) return true;
            }

            return false;
        }

        public int IndexOf(T element)
        {
            for (var i = 0; i < _capacity; i++)
            {
                if (element.Equals(_data[i])) return i;
            }

            return -1;
        }

        public ref T GetByRef(int index) => ref _data[index];

        public T[] ToArray()
        {
            var array = new T[_capacity];

            for (var i = 0; i < _capacity; i++)
            {
                array[i] = _data[i];
            }

            return array;
        }

        public struct Enumerator : IEnumerator<T>
        {
            object IEnumerator.Current => Current;

            public T Current => _list[_index];

            private readonly UnsafeList<T> _list;
            private int _index;

            public Enumerator(ref UnsafeList<T> list) : this()
            {
                _list = list;
                _index = -1;
            }

            public bool MoveNext()
            {
                ++_index;
                return _index < _list.Count;
            }

            public void Reset() => _index = -1;

            public void Dispose()
            {
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(ref this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        public readonly void Dispose()
        {
#if DEBUG_MODE
            if (!IsNotNull) throw new NullReferenceException();
#endif
            MemoryUtility.Free(_data, _allocator);
        }

        public T this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }
    }

    internal sealed class UnsafeListDebugView<T> where T : unmanaged, IEquatable<T>
    {
        private UnsafeList<T> _data;

        public UnsafeListDebugView(UnsafeList<T> array) => _data = array;

        public T[] Items => _data.ToArray();
    }
}