using System;
using System.Collections;
using System.Collections.Generic;
using DesertImage.ECS;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.Collections
{
    public unsafe struct UnsafeLinkedList<T> : IDisposable, IEnumerable<UnsafeLinkedList<T>.Node> where T : unmanaged
    {
        public bool IsNotNull { get; private set; }

        public struct Node
        {
            public int Index;
            public T Value;
            public int Previous;
            public int Next;
        }

        [NativeDisableUnsafePtrRestriction] private Node* _data;
        private UnsafeQueue<int> _freeIndexes;

        private readonly int _capacity;

        private int _first;
        private int _last;

        public UnsafeLinkedList(int capacity, Allocator allocator)
        {
            _capacity = capacity;

            _data = MemoryUtility.AllocateClearCapacity<Node>(capacity);
            _freeIndexes = new UnsafeQueue<int>(capacity, allocator);

            for (var i = _capacity - 1; i >= 0; i--)
            {
                _freeIndexes.Enqueue(i);
            }

            _first = -1;
            _last = -1;

            IsNotNull = true;
        }

        public void AddFirst(T value)
        {
            var node = new Node
            {
                Value = value,
                Previous = -1,
                Next = -1
            };

            var newIndex = GetNextFreeIndex();

            node.Index = newIndex;

            if (_first == -1)
            {
                _first = newIndex;
            }
            else
            {
                node.Next = _first;

                _data[_first].Previous = newIndex;
            }

            _data[newIndex] = node;
        }

        public void AddLast(T value)
        {
            var node = new Node
            {
                Value = value,
                Next = -1,
            };

            var newIndex = GetNextFreeIndex();

            node.Index = newIndex;

            if (_last == -1)
            {
                _last = newIndex;
            }
            else
            {
                node.Previous = _last;
                _data[_last].Next = newIndex;
            }

            _data[newIndex] = node;
        }

        public void RemoveFirst()
        {
            if (_first == -1) return;

            var node = _data[_first];

            _data[_first] = default;

            if (node.Next < 0) return;

            _first = node.Next;

            _data[node.Next].Previous = -1;
        }

        public void RemoveLast()
        {
            if (_last == -1) return;

            var node = _data[_last];

            _data[_last] = default;

            if (node.Previous < 0) return;

            _last = node.Previous;

            _data[node.Previous].Next = -1;
        }

        public ref Node GetFirst() => ref _data[_first];
        public bool HasFirst() => _first >= 0;

        public ref Node Get(int index) => ref _data[index];

        public void Clear()
        {
            _freeIndexes.Clear();

            for (var i = _capacity - 1; i >= 0; i--)
            {
                _data[i] = default;
                _freeIndexes.Enqueue(i);
            }

            _first = -1;
            _last = -1;
        }

        private int GetNextFreeIndex()
        {
            if (_freeIndexes.Count == 0) Resize(_capacity << 1);
            return _freeIndexes.Dequeue();
        }

        private void Resize(int newCapacity)
        {
            var oldCapacity = _capacity;

            _data = MemoryUtility.Resize(_data, oldCapacity, newCapacity);

            for (var i = oldCapacity; i < newCapacity; i++)
            {
                _freeIndexes.Enqueue(i);
            }
        }

        public void Dispose()
        {
            MemoryUtility.Free(_data);
            _freeIndexes.Dispose();
        }

        public struct Enumerator : IEnumerator<Node>
        {
            private readonly UnsafeLinkedList<T> _data;
            object IEnumerator.Current => Current;

            public Node Current => _data._data[_nodeIndex];

            private int _nodeIndex;

            public Enumerator(UnsafeLinkedList<T> data) : this()
            {
                _data = data;
                _nodeIndex = -1;
            }

            public bool MoveNext()
            {
                bool isHaveNext;

                if (_nodeIndex == -1)
                {
                    isHaveNext = _data._first >= 0;
                    _nodeIndex = _data._first;
                }
                else
                {
                    var node = _data._data[_nodeIndex];
                    _nodeIndex = node.Next;

                    isHaveNext = node.Next >= 0;
                }

                return isHaveNext;
            }

            public void Reset() => _nodeIndex = -1;

            public void Dispose()
            {
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<Node> IEnumerable<Node>.
            GetEnumerator() => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }
}