using System;
using System.Collections;
using System.Collections.Generic;
using DesertImage.ECS;
using Unity.Collections;

namespace DesertImage.Collections
{
    public struct Pair<TKey, TValue> where TKey : unmanaged where TValue : unmanaged
    {
        public TKey Key;
        public TValue Value;
    }

    public struct UnsafeDictionary<TKey, TValue> : IDisposable, IEnumerable<Pair<TKey, TValue>>
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
        internal UnsafeArray<UnsafeArray<Entry>> _entries;
        internal UnsafeArray<int> _lockIndexes;

        private int _entriesCapacity;

        public UnsafeDictionary(int capacity, Allocator allocator) : this()
        {
            _entriesCapacity = 5;

            _buckets = new UnsafeArray<int>(capacity, allocator, -1);
            _entries = new UnsafeArray<UnsafeArray<Entry>>(capacity * _entriesCapacity, allocator, default);
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
            return GetEntry(bucketNumber, hashCode) >= 0;
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
            var entryNumber = GetEntry(bucketNumber, hashCode);

            value = _entries[bucketNumber][entryNumber].Value;

            return true;
        }

        public ref TValue GetByRef(TKey key)
        {
            var hashCode = key.GetHashCode();

            var bucketNumber = GetBucketNumber(hashCode);
            var entryNumber = GetEntry(bucketNumber, hashCode);

            return ref _entries.Get(bucketNumber).Get(entryNumber).Value;
        }

        public void ResizeEntriesCapacity(int newCapacity)
        {
            for (var i = 0; i < _entries.Length; i++)
            {
                var bucketEntries = _entries[i];
                bucketEntries.Resize(newCapacity);
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
                
                ref var entry = ref _entries.Get(bucketNumber).Get(freeEntryIndex);
                entry = new Entry
                {
                    HashCode = hashCode,
                    Key = key,
                    Value = value
                };

                Count++;
            }
            _lockIndexes.Get(bucketNumber).Unlock();
        }

        private void Replace(TKey key, TValue value)
        {
#if DEBUG
            if (!IsNotNull) throw new Exception("Dictionary is null");
#endif

            var hashCode = key.GetHashCode();
            var bucketNumber = (hashCode & int.MaxValue) % _buckets.Length;

            _lockIndexes.Get(bucketNumber).Lock();
            {
                var entryNumber = GetEntry(bucketNumber, hashCode);

                if (entryNumber == -1) throw new NullReferenceException();

                ref var entry = ref _entries.Get(bucketNumber).Get(entryNumber);
                entry.Value = value;
            }
            _lockIndexes.Get(bucketNumber).Unlock();
        }

        private int GetFreeEntryIndex(int bucketNumber)
        {
            return GetFreeEntryIndex(bucketNumber, _entriesCapacity);
        }

        private int GetFreeEntryIndex(int bucketNumber, int entriesCapacity)
        {
            while (true)
            {
                var entriesNumber = bucketNumber * entriesCapacity;
                var lastBucketEntryNumber = entriesNumber + entriesCapacity - 1;

                for (var i = entriesNumber; i < lastBucketEntryNumber; i++)
                {
                    var bucketEntries = _entries[i];
                    for (var j = 0; j < bucketEntries.Length; j++)
                    {
                        if (bucketEntries[j].HashCode == 0) return i;
                    }
                }

                ResizeEntriesCapacity(entriesCapacity << 1);
            }
        }

        private int GetEntry(int bucketNumber, int hashCode)
        {
            var entriesNumber = bucketNumber * _entriesCapacity;
            var lastBucketEntryNumber = entriesNumber + _entriesCapacity - 1;

            for (var i = entriesNumber; i < lastBucketEntryNumber; i++)
            {
                var bucketEntries = _entries[i];
                foreach (var entry in bucketEntries)
                {
                    if (entry.HashCode == hashCode) return i;
                }
            }

            return -1;
        }

        private int GetBucketNumber(TKey key) => (key.GetHashCode() & int.MaxValue) % _buckets.Length;
        private int GetBucketNumber(int hashCode) => GetBucketNumber(hashCode, _buckets.Length);
        private int GetBucketNumber(int hashCode, int bucketLength) => (hashCode & int.MaxValue) % _buckets.Length;


        public TValue this[TKey key]
        {
            get => TryGetValue(key, out var value) ? value : default;
            set => Replace(key, value);
        }

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

                var entry = GetNextEntry(_lastEntryIndex + 1, out _lastEntryIndex);

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
                    var bucketEntries = _dictionary._entries[i];
                    for (var j = 0; j < bucketEntries.Length; j++)
                    {
                        var entry = bucketEntries[j];

                        if (entry.HashCode == 0) continue;

                        entryIndex = i;

                        return entry;
                    }
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