using System;
using DesertImage.ECS;
using Unity.Collections;

namespace DesertImage.Collections
{
    public unsafe struct UnsafeQueue<T> : IDisposable where T : unmanaged
    {
        public int Count { get; private set; }

        private UnsafeArray<T> _data;

        private int _lockIndex;

        public UnsafeQueue(int capacity, Allocator allocator) : this()
        {
            _data = new UnsafeArray<T>(capacity, allocator);
            Count = 0;
        }

        public void Enqueue(T element)
        {
            _lockIndex.Lock();
            {
                if (Count + 1 >= _data.Length)
                {
                    _data.Resize(Count << 1);
                }

                _data[Count] = element;
                Count++;
            }
            _lockIndex.Unlock();
        }

        public T Dequeue()
        {
            if (Count == 0) throw new Exception("No elements in queue");

            _lockIndex.Lock();
            var element = _data[Count - 1];
            Count--;
            _lockIndex.Unlock();

            return element;
        }

        public void Dispose() => _data.Dispose();
    }
}