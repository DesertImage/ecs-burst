using System;

namespace DesertImage.ECS
{
    [Serializable]
    public struct EntitiesGroup : IDisposable, IEquatable<EntitiesGroup>
    {
        public readonly int Id;

        public SparseSetInt Entities;

        public EntitiesGroup(int id)
        {
            Id = id;
            Entities = new SparseSetInt(ECSSettings.ComponentsDenseCapacity, ECSSettings.ComponentsSparseCapacity);
        }

        public void Add(int entityId) => Entities.Add(entityId);
        public void Remove(int entityId) => Entities.Remove(entityId);
        public bool Contains(int entityId) => Entities.Contains(entityId);

        public void Dispose() => Entities.Dispose();

        public bool Equals(EntitiesGroup other) => Id == other.Id;
        public override bool Equals(object obj) => obj is EntitiesGroup other && Equals(other);

        public override int GetHashCode() => Id;
    }
}