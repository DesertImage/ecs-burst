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
        internal struct Entry
        {
            public bool IsNotNull;

            public int HashCode;
            public TKey Key;
            public TValue Value;
        }

        public bool IsNotNull { get; }
        public int Count { get; private set; }

        private UnsafeLinkedList<Entry>* _entries;
        private int* _lockIndexes;
        private int _capacity;
        private Allocator _allocator;

        public UnsafeDictionary(int capacity, Allocator allocator) : this()
        {
            _capacity = capacity;
            _allocator = allocator;

            _lockIndexes = MemoryUtility.AllocateClear<int>(capacity * UnsafeUtility.SizeOf<int>(), allocator);
            _entries = MemoryUtility.AllocateClear<UnsafeLinkedList<Entry>>
            (
                capacity * UnsafeUtility.SizeOf<UnsafeLinkedList<Entry>>(),
                allocator
            );

            for (var i = 0; i < _capacity; i++)
            {
                _entries[i] = new UnsafeLinkedList<Entry>(3, Allocator.Persistent);
            }

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

            _lockIndexes[bucketNumber].Lock();
            {
                _entries[bucketNumber].Clear();
                Count--;
            }
            _lockIndexes[bucketNumber].Unlock();
        }

        public bool Contains(TKey key)
        {
#if DEBUG
            if (!IsNotNull) throw new Exception("Dictionary is null");
#endif
            if (Count == 0) return false;

            var hashCode = key.GetHashCode();
            var bucketNumber = GetBucketNumber(hashCode);
            return GetEntry(bucketNumber, hashCode).IsNotNull;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
#if DEBUG
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

            ref var node = ref _entries[bucketNumber].Get(entryNumber);
            ref var entry = ref node.Value;

            return ref entry.Value;
        }

        public void Resize(int newCapacity)
        {
            var oldEntries = _entries;

            MemoryUtility.Resize(ref _lockIndexes, _capacity, newCapacity);

            _entries = MemoryUtility.AllocateClear<UnsafeLinkedList<Entry>>
            (
                newCapacity * UnsafeUtility.SizeOf<UnsafeLinkedList<Entry>>(),
                _allocator
            );

            for (var i = 0; i < _capacity; i++)
            {
                var bucketEntries = oldEntries[i];
                if (!bucketEntries.IsNotNull) continue;

                foreach (var node in bucketEntries)
                {
                    var entry = node.Value;
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
#if DEBUG
            if (!IsNotNull) throw new Exception("Dictionary is null");
#endif

            var hashCode = key.GetHashCode();
            var bucketNumber = (hashCode & int.MaxValue) % _capacity;

            _lockIndexes[bucketNumber].Lock();
            {
                ref var linkedList = ref GetBucketEntries(bucketNumber);
                // var linkedList = _entries[bucketNumber];
                linkedList.AddFirst
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

        private ref UnsafeLinkedList<Entry> GetBucketEntries(int bucketNumber)
        {
            if (!_entries[bucketNumber].IsNotNull)
            {
                _entries[bucketNumber] = new UnsafeLinkedList<Entry>(3, Allocator.Persistent);
            }

            return ref _entries[bucketNumber];
        }

        private void Set(TKey key, TValue value)
        {
            var hashCode = key.GetHashCode();
            var bucketNumber = (hashCode & int.MaxValue) % _capacity;

            _lockIndexes[bucketNumber].Lock();
            {
                ref var linkedList = ref GetBucketEntries(bucketNumber);
                foreach (var node in linkedList)
                {
                    if (node.Value.HashCode != hashCode) continue;

                    var newEntry = new Entry
                    {
                        IsNotNull = true,
                        HashCode = hashCode,
                        Key = key,
                        Value = value
                    };

                    ref var refNode = ref linkedList.Get(node.Index);
                    refNode.Value = newEntry;

                    break;
                }
            }
            _lockIndexes[bucketNumber].Unlock();
        }

        private void Replace(TKey key, TValue value)
        {
#if DEBUG
            if (!IsNotNull) throw new Exception("Dictionary is null");
#endif

            var hashCode = key.GetHashCode();
            var bucketNumber = (hashCode & int.MaxValue) % _capacity;

            _lockIndexes[bucketNumber].Lock();
            {
                var entryNumber = GetEntryNumber(bucketNumber, hashCode);

                if (entryNumber == -1) throw new NullReferenceException();

                ref var node = ref _entries[bucketNumber].Get(entryNumber);
                node.Value.Value = value;
            }
            _lockIndexes[bucketNumber].Unlock();
        }

        private Entry GetEntry(int bucketNumber, int hashCode)
        {
            var bucketEntries = _entries[bucketNumber];
            foreach (var node in bucketEntries)
            {
                if (node.Value.HashCode == hashCode) return node.Value;
            }

            return default;
        }

        private int GetEntryNumber(int bucketNumber, int hashCode)
        {
            var bucketEntries = GetBucketEntries(bucketNumber);
            foreach (var node in bucketEntries)
            {
                if (node.Value.HashCode == hashCode) return node.Index;
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
                    var entry = _lastNode.Value;
                    return new Pair<TKey, TValue>
                    {
                        Key = entry.Key,
                        Value = entry.Value
                    };
                }
            }

            object IEnumerator.Current => Current;

            private int _bucketNumber;
            private UnsafeLinkedList<Entry>.Node _lastNode;

            public Enumerator(UnsafeDictionary<TKey, TValue> data) : this()
            {
                _data = data;
                _bucketNumber = 0;
            }

            public bool MoveNext()
            {
                if (_data.Count == 0) return false;

                var linkedList = _data._entries[_bucketNumber];

                if (!linkedList.IsNotNull)
                {
                    _bucketNumber++;
                }

                var isLastNodeNull = _lastNode is { Index: 0, Previous: 0, Next: 0 };
                if (isLastNodeNull)
                {
                    if (linkedList.HasFirst())
                    {
                        _lastNode = linkedList.GetFirst();
                    }
                    else
                    {
                        _bucketNumber++;
                        return true;
                    }
                }
                else if (_lastNode.Next >= 0)
                {
                    _lastNode = linkedList.Get(_lastNode.Next);
                }
                else
                {
                    _bucketNumber++;
                }

                return _bucketNumber < _data._capacity;
            }

            public void Reset() => _bucketNumber = 0;

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