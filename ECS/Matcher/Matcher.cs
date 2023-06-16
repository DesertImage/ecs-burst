using System.Collections.Generic;
using System.Linq;

namespace DesertImage.ECS
{
    public struct Matcher
    {
        public int Id;

        public SortedSet<int> Components { get; }
        public SortedSet<int> NoneOfComponents { get; }

        private readonly HashSet<int> _allOf;
        private readonly HashSet<int> _noneOf;
        private readonly HashSet<int> _anyOf;

        public Matcher(int id, HashSet<int> allOf, HashSet<int> noneOf, HashSet<int> anyOf)
        {
            Id = id;

            _allOf = allOf;
            _noneOf = noneOf;
            _anyOf = anyOf;

            Components = new SortedSet<int>(allOf.Concat(anyOf).Except(_noneOf).ToArray());
            NoneOfComponents = new SortedSet<int>(noneOf);
        }

        public bool Check(SortedSet<int> componentIds)
        {
            return HasNot(componentIds) && HasAll(componentIds) && HasAnyOf(componentIds);
        }

        private bool HasNot(ICollection<int> componentIds)
        {
            foreach (var id in _noneOf)
            {
                if (componentIds.Contains(id)) return false;
            }

            return true;
        }

        private bool HasAll(ICollection<int> componentIds)
        {
            foreach (var id in _allOf)
            {
                if (!componentIds.Contains(id)) return false;
            }

            return true;
        }

        private bool HasAnyOf(ICollection<int> componentIds)
        {
            if (_anyOf.Count == 0) return true;
            
            foreach (var id in _anyOf)
            {
                if (componentIds.Contains(id)) return true;
            }

            return false;
        }
    }
}