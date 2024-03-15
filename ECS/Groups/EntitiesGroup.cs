using System;
using System.Collections;
using System.Collections.Generic;
using DesertImage.Collections;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public unsafe struct EntitiesGroup : IDisposable, IEnumerable<int>
    {
        public ushort Id;

        public int Count => _entities->Count;
        public uint ComponentsHashCode { get; private set; }

        public UnsafeReadOnlyArray<uint> Values => new UnsafeReadOnlyArray<uint>(_entities->_dense, _entities->Count);

        [NativeDisableUnsafePtrRestriction] internal UnsafeUintHashSet* _components;
        [NativeDisableUnsafePtrRestriction] internal UnsafeList<uint>* _with;
        [NativeDisableUnsafePtrRestriction] internal UnsafeList<uint>* _noneOf;

        [NativeDisableUnsafePtrRestriction] private UnsafeUintSparseSet<uint>* _entities;
        [NativeDisableUnsafePtrRestriction] private World* _world;

        private byte _isPrimaryFilled;

        public EntitiesGroup(ushort id, World* world, int entitiesCapacity)
        {
            Id = id;

            _world = world;

            _components = MemoryUtility.AllocateInstance(new UnsafeUintHashSet(2, Allocator.Persistent));

            _with = MemoryUtility.AllocateInstance(new UnsafeList<uint>(2, Allocator.Persistent));
            _noneOf = MemoryUtility.AllocateInstance(new UnsafeList<uint>(2, Allocator.Persistent));

            _entities = MemoryUtility.AllocateInstance
            (
                new UnsafeUintSparseSet<uint>(entitiesCapacity / 2, entitiesCapacity)
            );

            ComponentsHashCode = default;

            _isPrimaryFilled = 0;
        }

        public UnsafeUintReadOnlySparseSet<T> GetComponents<T>() where T : unmanaged
        {
            return /*Count == 0 ? default : */_world->State->Components.GetComponents<T>();
        }

        public Entity GetEntity(uint id) => new Entity(id, _world);

        public EntitiesGroup With(uint componentId, long componentSize)
        {
            if (_isPrimaryFilled == 0)
            {
                Fill(componentId, componentSize);
            }
            else
            {
                FilterWith(componentId);
            }

            return this;
        }

        public EntitiesGroup None(uint componentId)
        {
            FilterNone(componentId);
            return this;
        }

        #region FILL/FILTER

        private void Fill(uint componentId, long componentSize)
        {
            _isPrimaryFilled = 1;

            ComponentsHashCode += componentId;

            _components->Add(componentId);
            _with->Add(componentId);

            ref var storage = ref _world->State->Components;
            ref var spareSet = ref storage.GetSparseSetOrInitialize(componentId, componentSize);

            for (var i = 0; i < spareSet.Count; i++)
            {
                var entityId = spareSet._keys[i];

                //TODO:refactor this
                Groups.AddEntityGroup(entityId, Id, _world->State);
                Add(entityId);
            }
        }

        private void FilterWith(uint componentId)
        {
#if DEBUG_MODE
            if (_with->Contains(componentId))
            {
                throw new Exception($"group {Id} already filtered by component {componentId}");
            }
#endif
            ComponentsHashCode += componentId;

            _components->Add(componentId);
            _with->Add(componentId);

            var count = Count;
            for (var i = count - 1; i >= 0; i--)
            {
                var entityId = _entities->_dense[i];

                if (_world->State->Components.Contains(entityId, componentId)) continue;

                //TODO:refactor this
                Groups.RemoveEntityGroup(entityId, Id, _world->State);
                Remove(entityId);
            }
        }

        private void FilterNone(uint componentId)
        {
            _components->Add(componentId);
            _noneOf->Add(componentId);

            ComponentsHashCode -= componentId;

            var count = Count;
            for (var i = count - 1; i >= 0; i--)
            {
                var entityId = _entities->_dense[i];

                if (!_world->State->Components.Contains(entityId, componentId)) continue;

                //TODO:refactor this
                Groups.RemoveEntityGroup(entityId, Id, _world->State);
                Remove(entityId);
            }
        }

        #endregion

        public void Add(uint entityId)
        {
            _entities->Set(entityId, entityId);
        }

        public void Remove(uint entityId)
        {
            _entities->Remove(entityId);
        }

        public bool IsValid(uint entityId)
        {
            var componentStorage = _world->State->Components;

            foreach (var componentId in *_with)
            {
                if (!componentStorage.Contains(entityId, componentId)) return false;
            }

            foreach (var componentId in *_noneOf)
            {
                if (componentStorage.Contains(entityId, componentId)) return false;
            }

            return true;
        }

        public bool Contains(uint entityId) => _entities->Contains(entityId);

        public void Dispose()
        {
#if DEBUG_MODE
            // if (Id == 0) return;
            if (Id == 0) throw new Exception("Group is null");
#endif
            _components->Dispose();
            _with->Dispose();
            _noneOf->Dispose();
            _entities->Dispose();

            MemoryUtility.Free(_components);
            MemoryUtility.Free(_with);
            MemoryUtility.Free(_noneOf);
            MemoryUtility.Free(_entities);

            _components = null;
            _with = null;
            _noneOf = null;
            _entities = null;
            _world = null;

            Id = 0;
        }

        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<int> IEnumerable<int>.GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        public struct Enumerator : IEnumerator<uint>
        {
            private EntitiesGroup _group;

            public uint Current => _group._entities->_dense[_index];
            object IEnumerator.Current => Current;

            private int _index;

            public Enumerator(EntitiesGroup group)
            {
                _group = group;
                _index = group.Count;
            }

            public bool MoveNext()
            {
                --_index;
                return _group.Count > 0 && _index >= 0;
            }

            public void Reset() => _index = -1;

            public void Dispose()
            {
            }
        }
    }
}