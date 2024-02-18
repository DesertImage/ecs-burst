using System;
using DesertImage.Collections;
using Unity.Collections;

namespace DesertImage.ECS
{
    public struct Matcher : IDisposable
    {
        public uint Id { get; }

        public UnsafeHashSet<uint> Components => _components;

        private readonly UnsafeHashSet<uint> _components;

        private UnsafeList<uint> _allOf;
        private UnsafeList<uint> _noneOf;
        private UnsafeList<uint> _anyOf;

        public Matcher(uint id, UnsafeList<uint> allOf, UnsafeList<uint> noneOf, UnsafeList<uint> anyOf)
        {
            Id = id;

            _allOf = allOf;
            _noneOf = noneOf;
            _anyOf = anyOf;

            _components = new UnsafeHashSet<uint>(allOf.Count + noneOf.Count + anyOf.Count, Allocator.Persistent);

            for (var i = 0; i < allOf.Count; i++)
            {
                _components.Add(allOf[i]);
            }

            for (var i = 0; i < noneOf.Count; i++)
            {
                _components.Add(noneOf[i]);
            }

            for (var i = 0; i < anyOf.Count; i++)
            {
                _components.Add(anyOf[i]);
            }
        }

        public bool Check(Entity entity) => HasNot(entity) && HasAll(entity) && HasAnyOf(entity);

        public bool HasNot(Entity entity)
        {
            for (var i = 0; i < _noneOf.Count; i++)
            {
                if (entity.Has(_noneOf[i])) return false;
            }

            return true;
        }

        public bool HasAll(Entity entity)
        {
            for (var i = 0; i < _allOf.Count; i++)
            {
                if (entity.Has(_allOf[i])) return false;
            }

            return true;
        }

        public bool HasAnyOf(Entity entity)
        {
            if (_anyOf.Count == 0) return true;

            for (var i = 0; i < _anyOf.Count; i++)
            {
                if (entity.Has(_anyOf[i])) return true;
            }

            return false;
        }

        //TODO: dispose all matchers
        public readonly void Dispose()
        {
            Components.Dispose();

            _allOf.Dispose();
            _anyOf.Dispose();
            _noneOf.Dispose();
        }
    }
}