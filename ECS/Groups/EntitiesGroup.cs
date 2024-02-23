using System;
using DesertImage.Collections;

namespace DesertImage.ECS
{
    [Serializable]
    public struct EntitiesGroup : IDisposable, IEquatable<EntitiesGroup>
    {
        public readonly ushort Id;

        public UnsafeUintSparseSet<uint> Entities;

        public EntitiesGroup(ushort id)
        {
            Id = id;
            Entities = new UnsafeUintSparseSet<uint>(50, ECSSettings.ComponentsEntitiesCapacity);
        }

        public void Add(uint entityId) => Entities.Set(entityId, entityId);

        public void Remove(uint entityId) => Entities.Remove(entityId);
        public bool Contains(uint entityId) => Entities.Contains(entityId);

        public readonly void Dispose() => Entities.Dispose();

        public bool Equals(EntitiesGroup other) => Id == other.Id;
        public override bool Equals(object obj) => obj is EntitiesGroup other && Equals(other);

        public override int GetHashCode() => Id;
    }
}