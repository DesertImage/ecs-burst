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
    [DebuggerTypeProxy(typeof(UnsafeHashSetDebugView<>))]
    public unsafe struct UnsafeHashSet<T> : IDisposable, IEnumerable<T> where T : unmanaged
    {
        internal struct Entry
        {
            public int HashCode;
            public T Value;
        }

        public bool IsNotNull { get; }

        public int Count { get; private set; }

        [NativeDisableUnsafePtrRestriction] private int* _buckets;
        [NativeDisableUnsafePtrRestriction] private Entry* _entries;
        [NativeDisableUnsafePtrRestriction] private int* _lockIndexes;

        private int _capacity;
        private int _entriesCapacity;
        private Allocator _allocator;

        public UnsafeHashSet(int capacity, Allocator allocator) : this()
        {
            _entriesCapacity = 3;
            _capacity = capacity;
            _allocator = allocator;

            var intSize = MemoryUtility.SizeOf<T>();
            var fullIntSize = capacity * intSize;

            _buckets = MemoryUtility.AllocateClear(fullIntSize, -1, allocator);
            _entries = MemoryUtility.AllocateClear<Entry>
            (
                capacity * _entriesCapacity * MemoryUtility.SizeOf<Entry>(),
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
#else
                return;
#endif
            }

            var bucketNumber = GetBucketNumber(key);

            _lockIndexes[bucketNumber].Lock();
            {
                var entryNumber = _buckets[bucketNumber];

                _entries[entryNumber] = default;
                _buckets[bucketNumber] = -1;

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

            var hashCode = key.GetHashCode();
            var bucketNumber = GetBucketNumber(hashCode);

            return GetEntry(bucketNumber, key, hashCode) >= 0;
        }

        public void Resize(int newSize)
        {
            var oldSize = _capacity;

            var intSize = MemoryUtility.SizeOf<T>();
            var fullIntSize = newSize & intSize;

            var bucketNew = MemoryUtility.AllocateClear(fullIntSize, -1, _allocator);
            var entriesNew = MemoryUtility.AllocateClear<Entry>
            (
                newSize * _entriesCapacity * MemoryUtility.SizeOf<Entry>(),
                _allocator
            );
            var lockIndexesNew = MemoryUtility.AllocateClear<int>(fullIntSize, _allocator);

            for (var i = 0; i < oldSize; i++)
            {
                var entryNumber = _buckets[i];

                if (entryNumber < 0) continue;

                var entry = _entries[entryNumber];

                var hashCode = entry.HashCode;

                var newBucketNumber = GetBucketNumber(hashCode);
                var newEntryNumber = GetFreeEntryIndex(newBucketNumber);

                bucketNew[newBucketNumber] = newEntryNumber;
                lockIndexesNew[newBucketNumber] = _lockIndexes[newBucketNumber];
                _entries[newEntryNumber] = entry;
            }

            MemoryUtility.Free(_buckets, _allocator);
            MemoryUtility.Free(_entries, _allocator);
            MemoryUtility.Free(_lockIndexes, _allocator);

            _buckets = bucketNew;
            _entries = entriesNew;
            _lockIndexes = lockIndexesNew;

            _capacity = newSize;
        }

        public void Dispose()
        {
            MemoryUtility.Free(_buckets, _allocator);
            MemoryUtility.Free(_entries, _allocator);
            MemoryUtility.Free(_lockIndexes, _allocator);

            _buckets = null;
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
                var freeEntryIndex = GetFreeEntryIndex(bucketNumber);

                _buckets[bucketNumber] = freeEntryIndex;
                _entries[freeEntryIndex] = new Entry
                {
                    HashCode = hashCode,
                    Value = value,
                };

                Count++;
            }
            _lockIndexes[bucketNumber].Unlock();
        }

        private int GetFreeEntryIndex(int bucketNumber)
        {
            var entriesNumber = bucketNumber * _entriesCapacity;
            var lastBucketEntryNumber = entriesNumber + _entriesCapacity - 1;

            for (var i = entriesNumber; i < lastBucketEntryNumber; i++)
            {
                var entry = _entries[i];

                if (entry.HashCode == 0) return i;
            }

            throw new Exception("no free entries");
            // return -1;
        }

        private int GetEntry(int bucketNumber, T key, int hashCode)
        {
            var entriesNumber = bucketNumber * _entriesCapacity;
            var lastBucketEntryNumber = entriesNumber + _entriesCapacity - 1;

            for (var i = entriesNumber; i < lastBucketEntryNumber; i++)
            {
                var entry = _entries[i];

                if (entry.HashCode == hashCode && key.Equals(entry.Value)) return i;
            }

            return -1;
        }

        private int GetBucketNumber(T key) => (key.GetHashCode() & int.MaxValue) % _capacity;
        private int GetBucketNumber(int hashCode) => (hashCode & int.MaxValue) % _capacity;

        public struct Enumerator : IEnumerator<T>
        {
            private readonly UnsafeHashSet<T> _data;

            public T Current => _data._entries[_counter].Value;
            object IEnumerator.Current => Current;

            private int _counter;
            private int _lastEntry;

            public Enumerator(UnsafeHashSet<T> data) : this()
            {
                _data = data;
                _counter = -1;
            }

            public bool MoveNext()
            {
                if(_data.Count == 0) return false;
                
                ++_counter;

                while (_data._entries[_counter].HashCode < 0)
                {
                    _counter++;
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

    internal sealed unsafe class UnsafeHashSetDebugView<T> where T : unmanaged
    {
        private readonly UnsafeHashSet<T> _data;

        public UnsafeHashSetDebugView(UnsafeHashSet<T> array) => _data = array;

        // public int[] Buckets => _data._buckets->ToArray();
        // public UnsafeHashSet<T>.Entry[] Entries => _data._entries->ToArray();
    }
}