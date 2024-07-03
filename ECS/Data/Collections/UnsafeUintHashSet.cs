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
    [DebuggerTypeProxy(typeof(UnsafeUintHashSetDebugView))]
    public unsafe struct UnsafeUintHashSet : IDisposable, IEnumerable<uint>
    {
        public bool IsNotNull { get; private set; }

        public int Count { get; private set; }

        [NativeDisableUnsafePtrRestriction] internal uint* _entries;

        internal int _capacity;
        private Allocator _allocator;

        public UnsafeUintHashSet(int capacity, Allocator allocator) : this()
        {
            _capacity = capacity;
            _allocator = allocator;

            _entries = MemoryUtility.AllocateClear
            (
                capacity * MemoryUtility.SizeOf<uint>(),
                0u,
                allocator
            );

            IsNotNull = true;

            Count = 0;
        }

        public void Add(uint key) => Insert(key);

        public void Remove(uint key)
        {
#if DEBUG_MODE
            if (!IsNotNull) throw new Exception("Dictionary is null");
#endif

            if (!Contains(key))
            {
#if DEBUG_MODE
                throw new Exception($"missing {key}");
#else
                return;
#endif
            }

            _entries[key] = 0u;

            Count--;
        }

        public bool Contains(uint key)
        {
#if DEBUG_MODE
            if (!IsNotNull) throw new Exception("HashSet is null");
#endif
            if (Count == 0 || key >= _capacity) return false;
            return _entries[key] > 0;
        }

        public void Resize(int newSize)
        {
        }

        public void Dispose()
        {
            IsNotNull = false;
            MemoryUtility.Free(_entries, _allocator);
            _entries = null;
        }

        private void Insert(uint value)
        {
#if DEBUG_MODE
            if (!IsNotNull) throw new Exception("HashSet is null");
#endif
            if (value >= _capacity)
            {
                var newSize = _capacity << 1;
                if (newSize <= value)
                {
                    newSize = (int)(value + 1);
                }

                _entries = MemoryUtility.Resize(_entries, _capacity, newSize);
                _capacity = newSize;
            }

            _entries[value] = value + 1;

            Count++;
        }

        public struct Enumerator : IEnumerator<uint>
        {
            private readonly UnsafeUintHashSet _data;

            public uint Current => _data._entries[_counter] - 1;
            object IEnumerator.Current => Current;

            private int _counter;
            private int _lastEntry;

            public Enumerator(UnsafeUintHashSet data) : this()
            {
                _data = data;
                _counter = -1;
            }

            public bool MoveNext()
            {
                ++_counter;

                while (_data._entries[_counter] == 0)
                {
                    _counter++;
                    if (_counter >= _data._capacity) return false;
                }

                return _counter < _data._capacity;
            }

            public void Reset() => _counter = -1;

            public void Dispose()
            {
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<uint> IEnumerable<uint>.GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }

    internal sealed unsafe class UnsafeUintHashSetDebugView
    {
        private readonly UnsafeUintHashSet _data;

        public UnsafeUintHashSetDebugView(UnsafeUintHashSet array) => _data = array;

        public uint[] Items => MemoryUtility.ToArray(_data._entries, _data._capacity);
    }
}