using System;
using DesertImage.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    [Serializable]
    public unsafe struct EntitiesGroup : IDisposable, IEquatable<EntitiesGroup>
    {
        public readonly ushort Id;

        public int Count => _entities->Count;

        [NativeDisableUnsafePtrRestriction] private UnsafeUintSparseSet<uint>* _entities;

        public EntitiesGroup(ushort id)
        {
            Id = id;
            _entities = MemoryUtility.Allocate
            (
                new UnsafeUintSparseSet<uint>(50, ECSSettings.ComponentsEntitiesCapacity)
            );
        }

        public void Add(uint entityId) => _entities->Set(entityId, entityId);
        public void Remove(uint entityId) => _entities->Remove(entityId);
        public bool Contains(uint entityId) => _entities->Contains(entityId);

        public void Dispose()
        {
            _entities->Dispose();
            MemoryUtility.Free(_entities);
        }

        public uint GetEntityId(int index) => _entities->_dense[index];

        public bool Equals(EntitiesGroup other) => Id == other.Id;
        public override bool Equals(object obj) => obj is EntitiesGroup other && Equals(other);

        public override int GetHashCode() => Id;
    }
}