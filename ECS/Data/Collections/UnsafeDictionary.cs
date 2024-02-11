using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;

namespace DesertImage.ECS
{
    public struct Pair<TKey, TValue> where TKey : unmanaged where TValue : unmanaged
    {
        public TKey Key;
        public TValue Value;
    }

    public unsafe struct UnsafeDictionary<TKey, TValue> : IDisposable, IEnumerable<Pair<TKey, TValue>>
        where TKey : unmanaged where TValue : unmanaged
    {
        internal struct Entry
        {
            public bool IsNull => HashCode == 0;

            public int HashCode;
            public TKey Key;
            public TValue Value;
        }

        public bool IsNotNull { get; }
        public int Count { get; private set; }

        internal UnsafeArray<int> _buckets;
        internal UnsafeArray<Entry> _entries;
        internal UnsafeArray<int> _lockIndexes;

        private int _entriesCapacity;

        public UnsafeDictionary(int capacity, Allocator allocator) : this()
        {
            _entriesCapacity = 5;

            _buckets = new UnsafeArray<int>(capacity, allocator, -1);
            _entries = new UnsafeArray<Entry>(capacity * _entriesCapacity, allocator, default);
            _lockIndexes = new UnsafeArray<int>(capacity, allocator, 0);

            IsNotNull = true;
        }

        public void Add(TKey key, TValue value) => Insert(key, value);

        public void Remove(TKey key)
        {
#if DEBUG
            if (!IsNotNull) throw new Exception("Dictionary is null");
#endif
            if (!Contains(key))
            {
#if DEBUG
                throw new Exception($"missing {key}");
#endif
                return;
            }

            var bucketNumber = GetBucketNumber(key);

            _lockIndexes.Get(bucketNumber).Lock();
            {
                var entryNumber = _buckets[bucketNumber];

                _entries[entryNumber] = default;
                _buckets[bucketNumber] = -1;

                Count--;
            }
            _lockIndexes.Get(bucketNumber).Unlock();
        }

        public bool Contains(TKey key)
        {
#if DEBUG
            if (!IsNotNull) throw new Exception("Dictionary is null");
#endif
            if (Count == 0) return false;

            var hashCode = key.GetHashCode();
            var bucketNumber = GetBucketNumber(hashCode);
            return GetEntry(bucketNumber, key, hashCode) >= 0;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
#if DEBUG
            if (!IsNotNull) throw new Exception("Dictionary is null");
#endif
            value = default;

            if (!Contains(key)) return false;

            var hashCode = key.GetHashCode();

            var bucketNumber = GetBucketNumber(hashCode);
            var entry = GetEntry(bucketNumber, key, hashCode);

            value = _entries[entry].Value;

            return true;
        }

        public void Resize(int newSize)
        {
            var oldSize = _buckets.Length;

            var bucketNew = new UnsafeArray<int>(newSize, Allocator.Persistent, default);
            var entriesNew = new UnsafeArray<Entry>(newSize * _entriesCapacity, Allocator.Persistent, default);
            var lockIndexesNew = new UnsafeArray<int>(newSize, Allocator.Persistent, default);

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

            _buckets.Dispose();
            _entries.Dispose();
            _lockIndexes.Dispose();

            _buckets = bucketNew;
            _entries = entriesNew;
            _lockIndexes = lockIndexesNew;

            for (var i = 0; i < _entries.Length; i++)
            {
                if (_entries[i].HashCode != 0) return;
            }
        }

        public void Dispose()
        {
            _buckets.Dispose();
            _entries.Dispose();
            _lockIndexes.Dispose();
        }

        private void Insert(TKey key, TValue value)
        {
#if DEBUG
            if (!IsNotNull) throw new Exception("Dictionary is null");
#endif

            var hashCode = key.GetHashCode();
            var bucketNumber = (hashCode & int.MaxValue) % _buckets.Length;

            _lockIndexes.Get(bucketNumber).Lock();
            {
                var freeEntryIndex = GetFreeEntryIndex(bucketNumber);

                _buckets[bucketNumber] = freeEntryIndex;
                _entries[freeEntryIndex] = new Entry
                {
                    HashCode = hashCode,
                    Key = key,
                    Value = value
                };

                Count++;
            }
            _lockIndexes.Get(bucketNumber).Unlock();
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

#if DEBUG
            throw new Exception("no free entries");
#endif
            return -1;
        }

        private int GetEntry(int bucketNumber, TKey key, int hashCode)
        {
            var entriesNumber = bucketNumber * _entriesCapacity;
            var lastBucketEntryNumber = entriesNumber + _entriesCapacity - 1;

            for (var i = entriesNumber; i < lastBucketEntryNumber; i++)
            {
                var entry = _entries[i];
                if (entry.HashCode == hashCode && key.Equals(entry.Key)) return i;
            }

            return -1;
        }

        private int GetBucketNumber(TKey key) => (key.GetHashCode() & int.MaxValue) % _buckets.Length;
        private int GetBucketNumber(int hashCode) => (hashCode & int.MaxValue) % _buckets.Length;

        public TValue this[TKey key] => TryGetValue(key, out var value) ? value : default;

        public struct Enumerator : IEnumerator<Pair<TKey, TValue>>
        {
            private readonly UnsafeDictionary<TKey, TValue> _dictionary;

            public Pair<TKey, TValue> Current
            {
                get
                {
                    var entry = GetNextEntry(_lastEntryIndex, out _lastEntryIndex);
                    return new Pair<TKey, TValue>
                    {
                        Key = entry.Key,
                        Value = entry.Value
                    };
                }
            }

            object IEnumerator.Current => Current;

            private int _counter;
            private int _lastEntryIndex;

            public Enumerator(UnsafeDictionary<TKey, TValue> dictionary) : this()
            {
                _dictionary = dictionary;
                _counter = -1;
                _lastEntryIndex = 0;
            }

            public bool MoveNext()
            {
                ++_counter;

                var entry = GetNextEntry(_lastEntryIndex, out _lastEntryIndex);

                return _counter < _dictionary.Count && !entry.IsNull;
            }

            public void Reset() => _counter = -1;

            public void Dispose()
            {
            }

            private Entry GetNextEntry(int startIndex, out int entryIndex)
            {
                entryIndex = -1;

                for (var i = startIndex; i < _dictionary._entries.Length; i++)
                {
                    var entry = _dictionary._entries[i];

                    if (entry.HashCode == 0) continue;

                    entryIndex = i;

                    return entry;
                }

                return default;
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<Pair<TKey, TValue>> IEnumerable<Pair<TKey, TValue>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }
}