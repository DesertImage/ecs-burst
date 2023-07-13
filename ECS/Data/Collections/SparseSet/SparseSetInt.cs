using System;
using System.Collections;
using System.Collections.Generic;

namespace DesertImage
{
    public class SparseSetInt : IEnumerable<int>
    {
        public int Count { get; private set; }

        private readonly int[] _dense;
        private readonly int[] _sparse;

        public int this[int index] => _dense[index];

        public SparseSetInt(int denseCapacity, int sparseCapacity)
        {
            _dense = new int[denseCapacity];
            _sparse = new int[sparseCapacity];
        }

        public void Add(int value)
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

        public void Remove(int value)
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

        public bool Contains(int value)
        {
            if(value < 0) return false;
            if(Count == 0) return false;
            
            return _sparse[value] < Count && _dense[_sparse[value]] == value;
        }

        public IEnumerator<int> GetEnumerator()
        {
            var i = 0;

            while (i < Count)
            {
                yield return _dense[i];
                i++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}