using System;
using DesertImage.Collections;
using Unity.Collections;

namespace DesertImage.ECS
{
    public readonly struct Matcher : IDisposable
    {
        public bool IsNull => Id == 0;

        public ushort Id { get; }

        public UnsafeUintSparseSet<uint> Components => _components;

        private readonly UnsafeUintSparseSet<uint> _components;

        private readonly UnsafeList<uint> _allOf;
        private readonly UnsafeList<uint> _noneOf;
        private readonly UnsafeList<uint> _anyOf;

        public Matcher(ushort id, UnsafeList<uint> allOf, UnsafeList<uint> noneOf, UnsafeList<uint> anyOf)
        {
            Id = id;

            _allOf = allOf;
            _noneOf = noneOf;
            _anyOf = anyOf;

            _components = new UnsafeUintSparseSet<uint>(allOf.Count + noneOf.Count + anyOf.Count);

            for (var i = 0; i < allOf.Count; i++)
            {
                var componentId = allOf[i];
                _components.Set(componentId, componentId);
            }

            for (var i = 0; i < noneOf.Count; i++)
            {
                var componentId = noneOf[i];
                _components.Set(componentId, componentId);
            }

            for (var i = 0; i < anyOf.Count; i++)
            {
                var componentId = anyOf[i];
                _components.Set(componentId, componentId);
            }
        }

        public bool Check(Entity entity) => HasNoneOf(entity) && HasAll(entity) && HasAnyOf(entity);

        public bool HasNoneOf(in Entity entity)
        {
            for (var i = 0; i < _noneOf.Count; i++)
            {
                if (entity.Has(_noneOf[i])) return false;
            }

            return true;
        }

        public bool HasAll(in Entity entity)
        {
            for (var i = 0; i < _allOf.Count; i++)
            {
                if (!entity.Has(_allOf[i])) return false;
            }

            return true;
        }

        public bool HasAnyOf(in Entity entity)
        {
            if (_anyOf.Count == 0) return true;

            for (var i = 0; i < _anyOf.Count; i++)
            {
                if (entity.Has(_anyOf[i])) return true;
            }

            return false;
        }

        public void Dispose()
        {
            Components.Dispose();

            _allOf.Dispose();
            _anyOf.Dispose();
            _noneOf.Dispose();
        }
    }
}