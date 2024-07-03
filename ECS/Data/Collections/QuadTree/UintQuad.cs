using System;
using Unity.Collections;
using UnityEngine;

namespace DesertImage.Collections
{
    public struct UintQuad : IDisposable, IEquatable<UintQuad>
    {
        public int Id;
        public int Parent;
        public int Depth;
        public int MaxDepth;

        internal int _pointsCapacity;

        public Bounds Bounds;

        public UnsafeUintSparseSet<UintEntry> Values;
        public UnsafeArray<int> _children;

        private int _halfSize;
        private int _fourthSize;

        private readonly Allocator _allocator;

        public UintQuad(int id, int parent, int depth, int maxDepth, Bounds bounds, int pointsCapacity = 5,
            Allocator allocator = Allocator.Persistent)
        {
            Id = id;

            Parent = parent;
            Depth = depth;
            MaxDepth = maxDepth;

            _pointsCapacity = pointsCapacity;

            Bounds = bounds;
            _halfSize = bounds.Size.x / 2;
            _fourthSize = bounds.Size.x / 4;

            Values = new UnsafeUintSparseSet<UintEntry>(pointsCapacity, pointsCapacity, allocator);
            _children = default;

            _allocator = allocator;
        }

        public bool Contains(Vector2Int position) => Bounds.Contains(position.x, position.y);

        public int Insert(uint element, Vector2Int position, ref UnsafeArray<UintQuad> allQuads,
            ref UnsafeUintSparseSet<int> elementToQuad)
        {
            if (_children.IsNotNull)
            {
                return InsertToChild(element, position, ref allQuads, ref elementToQuad);
            }

            if (Values.Count < _pointsCapacity || Depth >= MaxDepth)
            {
                Values.AddOrUpdate
                (
                    element,
                    new UintEntry
                    {
                        Value = element,
                        Position = position
                    }
                );

                elementToQuad.AddOrUpdate(element, Id);
                return Id;
            }
            
            Split(ref allQuads, ref elementToQuad);
   
            return InsertToChild(element, position, ref allQuads, ref elementToQuad);
        }

        private int InsertToChild(uint element, Vector2Int position, ref UnsafeArray<UintQuad> allQuads,
            ref UnsafeUintSparseSet<int> elementToQuad)
        {
            var center = Bounds.Center;

            var isOnLeft = position.x <= center.x;
            var isAbove = position.y >= center.y;

#if DEBUG_MODE
            if (!_children.IsNotNull) throw new NullReferenceException();
#endif
            ref var child = ref allQuads.Get(_children[isOnLeft ? (isAbove ? 0 : 1) : (isAbove ? 2 : 3)]);
#if DEBUG_MODE
            if (child.Id < 0) throw new NullReferenceException();
#endif
            return child.Insert(element, position, ref allQuads, ref elementToQuad);
        }

        public int GetQuadId(uint element, ref UnsafeArray<UintQuad> allQuads)
        {
            if (_children.IsNotNull)
            {
                for (var i = 0; i < _children.Length; i++)
                {
                    var id = allQuads[_children[i]].GetQuadId(element, ref allQuads);
                    if (id == -1) continue;
                    return id;
                }

                return -1;
            }

            return Values.Contains(element) ? Id : -1;
        }

        private void Split(ref UnsafeArray<UintQuad> allQuads, ref UnsafeUintSparseSet<int> elementToQuad)
        {
#if DEBUG_MODE
            if (_children.IsNotNull) throw new Exception("Quad already splitted");
            if (!Values.IsNotNull) throw new NullReferenceException();
#endif

            _children = new UnsafeArray<int>(4, Allocator.Persistent);

            var center = Bounds.Center;

            var topLeft = GetNewChild
            (
                new Vector2Int
                (
                    center.x - _fourthSize,
                    center.y + _fourthSize
                ),
                ref allQuads
            );

            var bottomLeft = GetNewChild
            (
                new Vector2Int
                (
                    center.x - _fourthSize,
                    center.y - _fourthSize
                ),
                ref allQuads
            );

            var topRight = GetNewChild
            (
                new Vector2Int
                (
                    center.x + _fourthSize,
                    center.y + _fourthSize
                ),
                ref allQuads
            );

            var bottomRight = GetNewChild
            (
                new Vector2Int
                (
                    center.x + _fourthSize,
                    center.y - _fourthSize
                ),
                ref allQuads
            );

            _children[0] = topLeft.Id;
            _children[1] = bottomLeft.Id;
            _children[2] = topRight.Id;
            _children[3] = bottomRight.Id;

            foreach (var entry in Values)
            {
                Insert(entry.Value, entry.Position, ref allQuads, ref elementToQuad);
            }

            Values.Clear();
        }

        private UintQuad GetNewChild(Vector2Int newBoundCenter, ref UnsafeArray<UintQuad> allQuads)
        {
            var id = -1;

            for (var i = 0; i < allQuads.Length; i++)
            {
                if (allQuads[i].Id != -1) continue;
                id = i;
                break;
            }

            if (id == -1)
            {
                throw new Exception("out of quads capacity");
                // id = allQuads.Length;
                // allQuads.Resize(allQuads.Length << 1, false);
                // for (var i = id; i < allQuads.Length; i++)
                // {
                //     allQuads[i] = new UintQuad { Id = -1 };
                // }
            }

            var newChild = new UintQuad
            (
                id,
                Id,
                Depth + 1,
                MaxDepth,
                new Bounds(newBoundCenter, new Vector2Int(_halfSize, _halfSize)),
                _pointsCapacity,
                _allocator
            );

            allQuads[id] = newChild;

            return newChild;
        }

        public void Dispose()
        {
            if (Values.IsNotNull)
            {
                Values.Dispose();
            }

            if (_children.IsNotNull)
            {
                _children.Dispose();
            }
        }

        public bool Equals(UintQuad other) => Id == other.Id;
    }
}