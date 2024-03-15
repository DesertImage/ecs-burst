using System;
using System.Collections;
using System.Collections.Generic;
using DesertImage.ECS;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.Collections
{
    public struct Pair<TKey, TValue> where TKey : unmanaged where TValue : unmanaged
    {
        public TKey Key;
        public TValue Value;
    }

    public unsafe struct UnsafeDictionary<TKey, TValue> : IDisposable, IEnumerable<Pair<TKey, TValue>>
        where TKey : unmanaged where TValue : unmanaged
    {
        internal struct Entry : IEquatable<Entry>
        {
            public bool IsNotNull;

            public int HashCode;
            public TKey Key;
            public TValue Value;

            public bool Equals(Entry other) => other.HashCode == HashCode;
        }

        public bool IsNotNull { get; }
        public int Count { get; private set; }

        [NativeDisableUnsafePtrRestriction] private UnsafeList<Entry>* _entries;
        [NativeDisableUnsafePtrRestriction] private int* _lockIndexes;
        private int _capacity;
        private readonly Allocator _allocator;

        public UnsafeDictionary(int capacity, Allocator allocator) : this()
        {
            _capacity = capacity;
            _allocator = allocator;

            _lockIndexes = MemoryUtility.AllocateClearCapacity<int>(capacity, allocator);
            _entries = MemoryUtility.AllocateClearCapacity<UnsafeList<Entry>>(capacity, allocator);

            for (var i = 0; i < _capacity; i++)
            {
                _entries[i] = new UnsafeList<Entry>(1, Allocator.Persistent);
            }

            IsNotNull = true;
        }

        public void Add(TKey key, TValue value) => Insert(key, value);

        public void Remove(TKey key)
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
                _entries[bucketNumber].Clear();
                Count--;
            }
            _lockIndexes[bucketNumber].Unlock();
        }

        public bool Contains(TKey key)
        {
#if DEBUG_MODE
            if (!IsNotNull) throw new Exception("Dictionary is null");
#endif
            if (Count == 0) return false;

            var hashCode = key.GetHashCode();
            var bucketNumber = GetBucketNumber(hashCode);
            return GetEntry(bucketNumber, hashCode).IsNotNull;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
#if DEBUG_MODE
            if (!IsNotNull) throw new Exception("Dictionary is null");
#endif
            value = default;

            var hashCode = key.GetHashCode();

            var bucketNumber = GetBucketNumber(hashCode);
            var entry = GetEntry(bucketNumber, hashCode);

            if (!entry.IsNotNull) return false;
            value = entry.Value;

            return true;
        }

        public ref TValue GetByRef(TKey key)
        {
            var hashCode = key.GetHashCode();

            var bucketNumber = GetBucketNumber(hashCode);
            var entryNumber = GetEntryNumber(bucketNumber, hashCode);

            ref var entry = ref _entries[bucketNumber].GetByRef(entryNumber);
            return ref entry.Value;
        }

        public void Resize(int newCapacity)
        {
            var oldEntries = _entries;

            _lockIndexes = MemoryUtility.Resize(_lockIndexes, _capacity, newCapacity);

            _entries = (UnsafeList<Entry>*)MemoryUtility.AllocateClearCapacity<UnsafeLinkedList<Entry>>
            (
                newCapacity,
                _allocator
            );

            for (var i = 0; i < _capacity; i++)
            {
                var bucketEntries = oldEntries[i];
                if (!bucketEntries.IsNotNull) continue;

                foreach (var entry in bucketEntries)
                {
                    Set(entry.Key, entry.Value);
                }
            }

            MemoryUtility.Free(oldEntries, _allocator);

            _capacity = newCapacity;
        }

        public void Dispose()
        {
            MemoryUtility.Free(_lockIndexes);

            for (var i = 0; i < _capacity; i++)
            {
                var linkedList = _entries[i];
                if (!linkedList.IsNotNull) continue;
                linkedList.Dispose();
            }

            MemoryUtility.Free(_entries);
        }

        private void Insert(TKey key, TValue value)
        {
#if DEBUG_MODE
            if (!IsNotNull) throw new Exception("Dictionary is null");
#endif

            var hashCode = key.GetHashCode();
            var bucketNumber = (hashCode & int.MaxValue) % _capacity;

            _lockIndexes[bucketNumber].Lock();
            {
                ref var list = ref GetBucketEntries(bucketNumber);
                list.Add
                (
                    new Entry
                    {
                        IsNotNull = true,
                        HashCode = hashCode,
                        Key = key,
                        Value = value
                    }
                );

                Count++;
            }
            _lockIndexes[bucketNumber].Unlock();

            if (Count >= _capacity)
            {
                Resize(Count << 1);
            }
        }

        private ref UnsafeList<Entry> GetBucketEntries(int bucketNumber)
        {
            if (!_entries[bucketNumber].IsNotNull)
            {
                _entries[bucketNumber] = new UnsafeList<Entry>(1, Allocator.Persistent);
            }

            return ref _entries[bucketNumber];
        }

        private void Set(TKey key, TValue value)
        {
            var hashCode = key.GetHashCode();
            var bucketNumber = (hashCode & int.MaxValue) % _capacity;

            _lockIndexes[bucketNumber].Lock();
            {
                ref var list = ref GetBucketEntries(bucketNumber);
                for (var i = 0; i < list.Count; i++)
                {
                    var entry = list[i];
                    if (entry.HashCode != hashCode) continue;

                    var newEntry = new Entry
                    {
                        IsNotNull = true,
                        HashCode = hashCode,
                        Key = key,
                        Value = value
                    };

                    list[i] = newEntry;
                    break;
                }
            }
            _lockIndexes[bucketNumber].Unlock();
        }

        private void Replace(TKey key, TValue value)
        {
#if DEBUG_MODE
            if (!IsNotNull) throw new Exception("Dictionary is null");
#endif

            var hashCode = key.GetHashCode();
            var bucketNumber = (hashCode & int.MaxValue) % _capacity;

            _lockIndexes[bucketNumber].Lock();
            {
                var entryNumber = GetEntryNumber(bucketNumber, hashCode);

                if (entryNumber == -1) throw new NullReferenceException();

                ref var entry = ref _entries[bucketNumber].GetByRef(entryNumber);
                entry.Value = value;
            }
            _lockIndexes[bucketNumber].Unlock();
        }

        private Entry GetEntry(int bucketNumber, int hashCode)
        {
            var bucketEntries = _entries[bucketNumber];
            for (var i = 0; i < bucketEntries.Count; i++)
            {
                var entry = bucketEntries[i];
                if (entry.HashCode == hashCode) return entry;
            }

            return default;
        }

        private int GetEntryNumber(int bucketNumber, int hashCode)
        {
            var bucketEntries = GetBucketEntries(bucketNumber);
            for (var i = 0; i < bucketEntries.Count; i++)
            {
                var entry = bucketEntries[i];
                if (entry.HashCode == hashCode) return i;
            }

            return -1;
        }

        private int GetBucketNumber(TKey key) => (key.GetHashCode() & int.MaxValue) % _capacity;
        private int GetBucketNumber(int hashCode) => GetBucketNumber(hashCode, _capacity);
        private int GetBucketNumber(int hashCode, int bucketLength) => (hashCode & int.MaxValue) % bucketLength;


        public TValue this[TKey key]
        {
            get => TryGetValue(key, out var value) ? value : default;
            set => Replace(key, value);
        }

        public struct Enumerator : IEnumerator<Pair<TKey, TValue>>
        {
            private readonly UnsafeDictionary<TKey, TValue> _data;

            public Pair<TKey, TValue> Current
            {
                get
                {
                    var entry = _data._entries[_bucketNumber][_entryNumber];
                    return new Pair<TKey, TValue>
                    {
                        Key = entry.Key,
                        Value = entry.Value
                    };
                }
            }

            object IEnumerator.Current => Current;

            private int _bucketNumber;
            private int _entryNumber;

            public Enumerator(UnsafeDictionary<TKey, TValue> data) : this()
            {
                _data = data;
                _bucketNumber = 0;
                _entryNumber = -1;
            }

            public bool MoveNext()
            {
                if (_data.Count == 0) return false;

                var list = _data._entries[_bucketNumber];

                while (!list.IsNotNull || list.Count == 0 || list.Count <= _entryNumber)
                {
                    list = _data._entries[++_bucketNumber];
                    _entryNumber = -1;
                }

                _entryNumber++;

                return _bucketNumber < _data._capacity;
            }

            public void Reset()
            {
                _bucketNumber = 0;
                _entryNumber = -1;
            }

            public void Dispose()
            {
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