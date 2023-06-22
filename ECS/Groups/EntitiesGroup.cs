using System;

namespace DesertImage.ECS
{
    [Serializable]
    public struct EntitiesGroup
    {
        public readonly int Id;

        public SparseSet<int> Entities;

        public EntitiesGroup(int id)
        {
            Id = id;
            Entities = new SparseSet<int>(ECSSettings.ComponentsDenseCapacity, ECSSettings.ComponentsSparseCapacity);
        }

        public void Add(int entityId) => Entities.Add(entityId, entityId);
        public void Remove(int entityId) => Entities.Remove(entityId);
        public bool Contains(int entityId) => Entities.Contains(entityId);
    }
}