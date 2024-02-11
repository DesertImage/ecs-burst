using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;

namespace DesertImage.ECS
{
    public struct SparseSetInt : IEnumerable<int>, INativeDisposable
    {
        public int Count { get; private set; }

        private UnsafeList<int> _dense;
        private UnsafeList<int> _sparse;
        private UnsafeList<int> _lockIndexes;

        public int this[int index] => _dense[index];

        public SparseSetInt(int denseCapacity, int sparseCapacity)
        {
            _dense = new UnsafeList<int>(denseCapacity, Allocator.Persistent);
            _sparse = new UnsafeList<int>(sparseCapacity, Allocator.Persistent);
            _lockIndexes = new UnsafeList<int>(denseCapacity, Allocator.Persistent, default);

            for (var i = 0; i < denseCapacity; i++)
            {
                _dense.Add(default);
            }

            for (var i = 0; i < sparseCapacity; i++)
            {
                _sparse.Add(default);
            }
            
            for (var i = 0; i < denseCapacity; i++)
            {
                _lockIndexes.Add(default);
            }

            Count = 0;
        }

        public void Add(int value)
        {
            _lockIndexes.GetByRef(Count).Lock();
            {
                if (Contains(value))
                {
#if DEBUG
                    throw new Exception($"SparseSet already contains {value}");
#else
                return;
#endif
                }

                _sparse[value] = Count;
                _dense[Count] = value;

                Count++;
            }
            _lockIndexes.GetByRef(Count).Unlock();
        }

        public void Remove(int value)
        {
            _lockIndexes.GetByRef(Count).Lock();
            {
                if (!Contains(value))
                {
#if DEBUG
                    throw new Exception($"SparseSet not contains {value}");
#else
                return;
#endif
                }

                var last = _dense[Count - 1];

                _dense[_sparse[value]] = last;
                _sparse[last] = _sparse[value];

                Count--;
            }
            _lockIndexes.GetByRef(Count).Unlock();
        }

        public bool Contains(int value)
        {
            if (value < 0) return false;
            if (Count == 0) return false;

            return _sparse[value] < Count && _dense[_sparse[value]] == value;
        }

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<int> IEnumerable<int>.GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose()
        {
            _dense.Dispose();
            _sparse.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            _dense.Dispose();
            _sparse.Dispose();
            return default;
        }

        public struct Enumerator : IEnumerator<int>
        {
            private SparseSetInt _data;

            public int Current => _data[_counter];
            object IEnumerator.Current => Current;

            private int _counter;

            public Enumerator(SparseSetInt data) : this()
            {
                _data = data;
                _counter = -1;
            }

            public bool MoveNext()
            {
                ++_counter;
                return _counter < _data.Count;
            }

            public void Reset() => _counter = -1;


            public void Dispose()
            {
            }
        }
    }
}