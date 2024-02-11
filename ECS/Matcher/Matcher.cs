using System;
using DesertImage.Collections;
using DesertImage.ECS;
using Unity.Collections;

namespace DesertImage.ECS
{
    public struct Matcher : IDisposable
    {
        public int Id;

        public UnsafeArray<int> Components => _components;

        public readonly UnsafeArray<int> NoneOfComponents => _noneOfComponents;

        private readonly UnsafeArray<int> _components;
        private readonly UnsafeArray<int> _noneOfComponents;

        private UnsafeList<int> _allOf;
        private UnsafeList<int> _noneOf;
        private UnsafeList<int> _anyOf;

        public Matcher(int id, UnsafeList<int> allOf, UnsafeList<int> noneOf, UnsafeList<int> anyOf)
        {
            Id = id;

            _allOf = allOf;
            _noneOf = noneOf;
            _anyOf = anyOf;

            _components = new UnsafeArray<int>(allOf.Count + noneOf.Count + anyOf.Count, Allocator.Persistent);
            _noneOfComponents = new UnsafeArray<int>(_noneOf.Count, Allocator.Persistent);

            for (var i = 0; i < allOf.Count; i++)
            {
                _components[i] = allOf[i];
            }

            for (var i = 0; i < noneOf.Count; i++)
            {
                _components[i + _allOf.Count] = noneOf[i];
                _noneOfComponents[i] = noneOf[i];
            }

            for (var i = 0; i < anyOf.Count; i++)
            {
                _components[i + _allOf.Count + noneOf.Count] = anyOf[i];
            }
        }

        public bool Check(UnsafeSparseSet<int> componentIds)
        {
            return HasNot(componentIds) && HasAll(componentIds) && HasAnyOf(componentIds);
        }

        private bool HasNot(UnsafeSparseSet<int> componentIds)
        {
            for (var i = 0; i < _noneOf.Count; i++)
            {
                if (componentIds.Contains(_noneOf[i])) return false;
            }

            return true;
        }

        private bool HasAll(UnsafeSparseSet<int> componentIds)
        {
            for (var i = 0; i < _allOf.Count; i++)
            {
                if (componentIds.Contains(_allOf[i])) return false;
            }

            return true;
        }

        private bool HasAnyOf(UnsafeSparseSet<int> componentIds)
        {
            if (_anyOf.Count == 0) return true;

            for (var i = 0; i < _anyOf.Count; i++)
            {
                if (componentIds.Contains(_anyOf[i])) return true;
            }

            return false;
        }

        //TODO: dispose all matchers
        public void Dispose()
        {
            Components.Dispose();
            NoneOfComponents.Dispose();

            _allOf.Dispose();
            _anyOf.Dispose();
            _noneOf.Dispose();
        }
    }
}