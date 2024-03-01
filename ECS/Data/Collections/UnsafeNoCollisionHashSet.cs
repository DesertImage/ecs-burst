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
    [DebuggerTypeProxy(typeof(UnsafeNoCollisionHashSetDebugView<>))]
    public unsafe struct UnsafeNoCollisionHashSet<T> : IDisposable, IEnumerable<T> where T : unmanaged
    {
        internal struct Entry
        {
            public static Entry Default => new Entry { HashCode = -1 };

            public int HashCode;
            public T Value;
        }

        public bool IsNotNull { get; }

        public int Count { get; private set; }

        [NativeDisableUnsafePtrRestriction] internal Entry* _entries;
        [NativeDisableUnsafePtrRestriction] private int* _lockIndexes;

        internal int _capacity;
        private Allocator _allocator;

        public UnsafeNoCollisionHashSet(int capacity, Allocator allocator) : this()
        {
            _capacity = capacity;
            _allocator = allocator;

            var intSize = MemoryUtility.SizeOf<T>();
            var fullIntSize = capacity * intSize;

            _entries = MemoryUtility.AllocateClear
            (
                capacity * MemoryUtility.SizeOf<Entry>(),
                Entry.Default,
                allocator
            );

            _lockIndexes = MemoryUtility.AllocateClear<int>(fullIntSize, allocator);

            IsNotNull = true;

            Count = 0;
        }

        public void Add(T key) => Insert(key);

        public void Remove(T key)
        {
#if DEBUG_MODE
            if (!IsNotNull) throw new Exception("Dictionary is null");
#endif

            if (!Contains(key))
            {
#if DEBUG_MODE
                throw new Exception($"missing {key}");
#endif
                return;
            }

            var bucketNumber = GetBucketNumber(key);

            _lockIndexes[bucketNumber].Lock();
            {
                _entries[bucketNumber] = Entry.Default;

                Count--;
            }
            _lockIndexes[bucketNumber].Unlock();
        }

        public bool Contains(T key)
        {
#if DEBUG_MODE
            if (!IsNotNull) throw new Exception("HashSet is null");
#endif
            if (Count == 0) return false;

            var bucketNumber = GetBucketNumber(key);
            return _entries[bucketNumber].HashCode >= 0;
        }

        public void Resize(int newSize)
        {
            var oldSize = _capacity;

            var intSize = MemoryUtility.SizeOf<T>();
            var fullIntSize = newSize * intSize;

            var entriesNew = MemoryUtility.AllocateClear
            (
                newSize * MemoryUtility.SizeOf<Entry>(),
                Entry.Default,
                _allocator
            );

            var lockIndexesNew = MemoryUtility.AllocateClear<int>(fullIntSize, _allocator);

            for (var i = 0; i < oldSize; i++)
            {
                var entry = _entries[i];

                var hashCode = entry.HashCode;

                if (hashCode < 0) continue;

                var newBucketNumber = GetBucketNumber(hashCode);

                lockIndexesNew[newBucketNumber] = _lockIndexes[i];
                entriesNew[newBucketNumber] = entry;
            }

            MemoryUtility.Free(_entries, _allocator);
            MemoryUtility.Free(_lockIndexes, _allocator);

            _entries = entriesNew;
            _lockIndexes = lockIndexesNew;

            _capacity = newSize;
        }

        public void Dispose()
        {
            MemoryUtility.Free(_entries, _allocator);
            MemoryUtility.Free(_lockIndexes, _allocator);

            _entries = null;
            _lockIndexes = null;
        }

        private void Insert(T value)
        {
#if DEBUG_MODE
            if (!IsNotNull) throw new Exception("Dictionary is null");
#endif
            var hashCode = value.GetHashCode();
            var bucketNumber = (hashCode & int.MaxValue) % _capacity;

            _lockIndexes[bucketNumber].Lock();
            {
                _entries[bucketNumber] = new Entry
                {
                    HashCode = hashCode,
                    Value = value,
                };

                Count++;

                if (Count >= _capacity)
                {
                    Resize(_capacity << 1);
                }
            }
            _lockIndexes[bucketNumber].Unlock();
        }

        private int GetBucketNumber(T key) => (key.GetHashCode() & int.MaxValue) % _capacity;
        private int GetBucketNumber(int hashCode) => (hashCode & int.MaxValue) % _capacity;

        public struct Enumerator : IEnumerator<T>
        {
            private readonly UnsafeNoCollisionHashSet<T> _data;

            public T Current => _data._entries[_counter].Value;
            object IEnumerator.Current => Current;

            private int _counter;
            private int _lastEntry;

            public Enumerator(UnsafeNoCollisionHashSet<T> data) : this()
            {
                _data = data;
                _counter = -1;
            }

            public bool MoveNext()
            {
                ++_counter;
                
                while (_data._entries[_counter].HashCode < 0)
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

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }

    internal sealed unsafe class UnsafeNoCollisionHashSetDebugView<T> where T : unmanaged
    {
        private readonly UnsafeNoCollisionHashSet<T> _data;

        public UnsafeNoCollisionHashSetDebugView(UnsafeNoCollisionHashSet<T> array) => _data = array;

        public UnsafeNoCollisionHashSet<T>.Entry[] Items => MemoryUtility.ToArray(_data._entries, _data._capacity);
        // public UnsafeHashSet<T>.Entry[] Entries => _data._entries->ToArray();
    }
}