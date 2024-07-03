using System;
using Unity.Collections;
using UnityEngine;

namespace DesertImage.Collections
{
    public struct UnsafeUintQuadTree : IDisposable
    {
        public UnsafeArray<UintQuad> Quads;
        public UnsafeUintSparseSet<int> ElementToQuad;
        private readonly int _maxPointsInQuad;

        private readonly Allocator _allocator;

        public UnsafeUintQuadTree(Bounds bounds, int maxDepth = 14, int maxPointsInQuad = 5, int quadsCapacity = 200,
            Allocator allocator = Allocator.Persistent)
        {
            Quads = new UnsafeArray<UintQuad>(quadsCapacity, allocator, new UintQuad { Id = -1 });
            _maxPointsInQuad = maxPointsInQuad;
            Quads[0] = new UintQuad(0, -1, 0, maxDepth, bounds, _maxPointsInQuad, allocator);

            ElementToQuad = new UnsafeUintSparseSet<int>(quadsCapacity, allocator);

            _allocator = allocator;
        }

        public int Insert(uint element, Vector2Int position)
        {
            ref var root = ref Quads.Get(0);
#if DEBUG_MODE
            if (root.Parent != -1 || root.Id != 0) throw new NullReferenceException();
#endif
            if (!root.Bounds.Contains(position)) return -1;

            var insert = root.Insert(element, position, ref Quads, ref ElementToQuad);

            return insert;
        }

        public void Update(uint element, Vector2Int position)
        {
#if DEBUG_MODE
            if(!ElementToQuad.Contains(element)) throw new NullReferenceException();
#endif
            var quadId = ElementToQuad.Read(element);

            ref var quad = ref Quads.Get(quadId);

#if !DEBUG_MODE
            if (quad is { Id: 0, _children: { IsNotNull: true } }) throw new NullReferenceException();
#endif

            if (quad.Bounds.Contains(position))
            {
                ref var entry = ref quad.Values.Get(element);
                entry.Position = position;
                return;
            }

            quad.Values.Remove(element);

            if (quad.Parent >= 0)
            {
                ref var parentQuad = ref Quads.Get(quad.Parent);
                if (parentQuad.Bounds.Contains(position))
                {
                    parentQuad.Insert(element, position, ref Quads, ref ElementToQuad);
                    return;
                }
            }

            Insert(element, position);
        }

        public UnsafeArray<uint> GetAllNeighbours(uint element, Allocator allocator)
        {
            var quad = Quads.Get(GetQuadId(element));

            var count = quad.Values.Count - 1;

            if (count < 0)
            {
                count = 0;
            }

            var array = new UnsafeArray<uint>(count, allocator);

            var index = 0;
            foreach (var entry in quad.Values)
            {
                if (entry.Value == element) continue;
                array[index] = entry.Value;
                index++;
            }

            return array;
        }

        public UnsafeArray<uint> GetAllIntersected(Bounds bound, Allocator allocator)
        {
            var list = new UnsafeList<uint>(_maxPointsInQuad * 5, allocator);
            GetIntersectedPoints(ref list, 0, bound);
            return list.ToUnsafeArray();
        }

        private UnsafeList<uint> GetIntersectedPoints(ref UnsafeList<uint> list, int quadId, Bounds bound)
        {
            var quad = Quads[quadId];

            if (quad.Bounds.Intersects(bound))
            {
                if (quad._children.IsNotNull)
                {
                    foreach (var childId in quad._children)
                    {
                        GetIntersectedPoints(ref list, childId, bound);
                    }
                }
                else
                {
                    foreach (var entry in quad.Values)
                    {
                        if(!bound.Contains(entry.Position)) continue;
                        list.Add(entry.Value);
                    }
                }
            }

            return list;
        }

        public void Clear()
        {
            ref var mainQuad = ref Quads.Get(0);
            if (mainQuad._children.IsNotNull)
            {
                mainQuad._children.Dispose();
                mainQuad.Values = new UnsafeUintSparseSet<UintEntry>(_maxPointsInQuad, _allocator);
            }
            else
            {
                mainQuad.Values.Clear();
            }

            for (var i = 1; i < Quads.Length; i++)
            {
                if (Quads[i].Id == -1) continue;
                Quads[i].Dispose();
                Quads[i] = new UintQuad { Id = -1 };
            }
        }

        private int GetQuadId(uint element) => ElementToQuad[element];

        public void Dispose()
        {
            ElementToQuad.Dispose();

            foreach (var quad in Quads)
            {
                if (quad.Id == -1) continue;
                quad.Dispose();
            }

            Quads.Dispose();
        }
    }
}