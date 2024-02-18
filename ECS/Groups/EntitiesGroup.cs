using System;
using DesertImage.Collections;

namespace DesertImage.ECS
{
    [Serializable]
    public struct EntitiesGroup : IDisposable, IEquatable<EntitiesGroup>
    {
        public readonly uint Id;

        public UnsafeSparseSet<uint> Entities;

        public EntitiesGroup(uint id)
        {
            Id = id;
            Entities = new UnsafeSparseSet<uint>(50, ECSSettings.ComponentsEntitiesCapacity);
        }

        public void Add(uint entityId) => Entities.Add((int)entityId, entityId);

        public void Remove(uint entityId) => Entities.Remove((int)entityId);
        public bool Contains(uint entityId) => Entities.Contains((int)entityId);

        public readonly void Dispose() => Entities.Dispose();

        public bool Equals(EntitiesGroup other) => Id == other.Id;
        public override bool Equals(object obj) => obj is EntitiesGroup other && Equals(other);

        public override int GetHashCode() => (int)Id;
    }
}