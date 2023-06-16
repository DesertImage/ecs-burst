using System;
using System.Collections.Generic;

namespace DesertImage.ECS
{
    [Serializable]
    public struct EntitiesGroup
    {
        public readonly int Id;
        
        //TODO: use Sparse Set
        public readonly List<int> Entities;
        public readonly HashSet<int> EntitiesHashSet;

        public EntitiesGroup(int id)
        {
            Id = id;
            Entities = new List<int>();
            EntitiesHashSet = new HashSet<int>();
        }

        public void Add(int entityId)
        {
            Entities.Add(entityId);
            EntitiesHashSet.Add(entityId);
        }

        public void Remove(int entityId)
        {
            Entities.Remove(entityId);
            EntitiesHashSet.Remove(entityId);
        }

        public bool Contains(int entityId) => EntitiesHashSet.Contains(entityId);
    }
}