using System;
using System.Collections;
using System.Collections.Generic;
using DesertImage.Collections;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public unsafe struct EntitiesGroup : IDisposable, IEnumerable<Entity>
    {
        public ushort Id;

        public int Count => _entities->Count;
        public uint ComponentsHashCode { get; private set; }

        private void** _buffer;
        private long _bufferSize;

        private UnsafeUintSparseSet<uint> _componentIndexes;

        internal UnsafeUintHashSet* _components;
        internal UnsafeList<uint> _with;
        internal UnsafeList<uint> _noneOf;

        private UnsafeUintSparseSet<uint>* _entities;

        private int _entitiesCapacity;
        private int _componentsCapacity;

        private World* _world;

        private byte _isPrimaryFilled;

        public EntitiesGroup(ushort id, World* world, int entitiesCapacity, int componentsCapacity)
        {
            Id = id;

            _world = world;
            _entitiesCapacity = entitiesCapacity;
            _componentsCapacity = componentsCapacity;

            _bufferSize = entitiesCapacity * componentsCapacity * IntPtr.Size;

            _buffer = (void**)UnsafeUtility.Malloc(_bufferSize, UnsafeUtility.AlignOf<byte>(), Allocator.Persistent);
            MemoryUtility.Clear(_buffer, _bufferSize);

            var hashSet = new UnsafeUintHashSet(2, Allocator.Persistent);
            _components = MemoryUtility.AllocateInstance
            (
                in hashSet
            );

            _componentIndexes = new UnsafeUintSparseSet<uint>(2);

            _with = new UnsafeList<uint>(2, Allocator.Persistent);
            _noneOf = new UnsafeList<uint>(2, Allocator.Persistent);

            var sparseSet = new UnsafeUintSparseSet<uint>(entitiesCapacity / 2, entitiesCapacity);
            _entities = MemoryUtility.AllocateInstance
            (
                in sparseSet
            );

            ComponentsHashCode = default;

            _isPrimaryFilled = 0;
        }

        public UnsafeArray<T> GetComponents<T>() where T : unmanaged
        {
            var componentId = ComponentTools.GetComponentId<T>();
#if DEBUG_MODE
            if (!_componentIndexes.Contains(componentId))
            {
                throw new Exception($"Group doesn't store components {typeof(T)} ");
            }
#endif
            var componentIndex = _componentIndexes.Read(componentId);
            var data = _buffer[componentIndex];
            return new UnsafeArray<T>((T*)data, Count, Allocator.Persistent);
        }

        public Entity GetEntity(int index) => new Entity(_entities->_dense[index], _world);

        internal EntitiesGroup With(uint componentId, long componentSize)
        {
            if (_isPrimaryFilled == 0)
            {
                Fill(componentId, componentSize);
            }
            else
            {
                FilterWith(componentId, componentSize);
            }

            return this;
        }

        public EntitiesGroup None(uint componentId)
        {
            FilterNone(componentId);
            return this;
        }

        private void Resize(int entitiesCapacity, int componentsCapacity)
        {
            long newBufferSize = entitiesCapacity * componentsCapacity * IntPtr.Size;

            var newBuffer = (void**)UnsafeUtility.Malloc
            (
                newBufferSize,
                UnsafeUtility.AlignOf<byte>(),
                Allocator.Persistent
            );

            MemoryUtility.Clear(newBuffer, _bufferSize);

            for (var i = 0; i < _componentIndexes.Count; i++)
            {
                var componentIndex = _componentIndexes._dense[i];
                for (var j = 0; j < _entitiesCapacity; j++)
                {
                    newBuffer[componentIndex * entitiesCapacity + j] = _buffer[componentIndex * _entitiesCapacity + j];
                }
            }

            MemoryUtility.Free(_buffer);

            _buffer = newBuffer;
            _entitiesCapacity = entitiesCapacity;
            _componentsCapacity = componentsCapacity;
            _bufferSize = newBufferSize;
        }

        #region FILL/FILTER

        private void Fill(uint componentId, long componentSize)
        {
            _isPrimaryFilled = 1;

            ComponentsHashCode += componentId;

            _componentIndexes.Set(componentId, 0);
            _components->Add(componentId);
            _with.Add(componentId);

            ref var storage = ref _world->State->Components;

            if (!storage.ContainsKey(componentId)) return;

            ref var spareSet = ref storage.GetSparseSet(componentId);

            for (var i = spareSet.Count - 1; i >= 0; i--)
            {
                var entityId = spareSet._keys[i];

                //TODO:refactor this
                Groups.AddEntityGroup(entityId, Id, _world->State);
                Add(entityId);
            }

            InitComponent(componentId, componentSize);
        }

        private void FilterWith(uint componentId, long componentSize)
        {
#if DEBUG_MODE
            if (_with.Contains(componentId))
            {
                throw new Exception($"group {Id} already filtered by component {componentId}");
            }
#endif
            ComponentsHashCode += componentId;

            _componentIndexes.Set(componentId, (uint)_with.Count);
            _components->Add(componentId);
            _with.Add(componentId);

            if (_with.Count >= _componentsCapacity) Resize(_entitiesCapacity, _componentsCapacity + 1);

            var count = Count;
            for (var i = count - 1; i >= 0; i--)
            {
                var entityId = _entities->_dense[i];

                if (_world->State->Components.Contains(entityId, componentId)) continue;

                //TODO:refactor this
                Groups.RemoveEntityGroup(entityId, Id, _world->State);
                Remove(entityId);
            }

            count = Count;
            for (var i = count - 1; i >= 0; i--)
            {
                FillComponent(_entities->_dense[i], componentId);
            }

            InitComponent(componentId, componentSize);
        }

        private void FilterNone(uint componentId)
        {
            _components->Add(componentId);
            _noneOf.Add(componentId);

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

        private void InitComponent(uint componentId, long componentSize)
        {
            var componentIndex = _componentIndexes.Read(componentId);
            var ptr = _world->State->Components.GetSparseSetOrInitialize(componentId, componentSize).GetPtr();
            _buffer[componentIndex] = ptr;
        }

        private void FillComponent(uint entityId, uint componentId)
        {
            var componentIndex = _componentIndexes.Read(componentId);
            var ptr = _world->State->Components.GetSparseSet(componentId).GetPtr(entityId);
            _buffer[componentIndex + entityId] = ptr;
        }

        #endregion

        public void Add(uint entityId)
        {
            var index = Count;

            if (index > _entitiesCapacity) Resize(_entitiesCapacity << 1, _componentsCapacity);

            foreach (var componentId in _with)
            {
                FillComponent(entityId, componentId);
            }

            _entities->Set(entityId, entityId);
        }

        public void Remove(uint entityId)
        {
            foreach (var componentId in _with)
            {
                var componentIndex = _componentIndexes.Read(componentId) * _entitiesCapacity;

                //TODO recheck. Probably don't work
                if (Count > 1)
                {
                    _buffer[componentIndex + entityId] = _buffer[componentIndex + (Count - 1)];
                }
                else
                {
                    _buffer[componentIndex + entityId] = (void*)IntPtr.Zero;
                }
            }

            _entities->Remove(entityId);
        }

        public bool IsValid(uint entityId)
        {
            var componentStorage = _world->State->Components;

            foreach (var componentId in _with)
            {
                if (!componentStorage.Contains(entityId, componentId)) return false;
            }

            foreach (var componentId in _noneOf)
            {
                if (componentStorage.Contains(entityId, componentId)) return false;
            }

            return true;
        }

        public bool ContainsKey(uint componentId) =>
            componentId < _entitiesCapacity && _componentIndexes.Contains(componentId);

        public bool Contains(uint entityId) => _entities->Contains(entityId);

        public void Dispose()
        {
            _components->Dispose();
            _componentIndexes.Dispose();
            _with.Dispose();
            _noneOf.Dispose();

            _entities->Dispose();

            MemoryUtility.Free(_buffer);
            MemoryUtility.Free(_components);
            MemoryUtility.Free(_entities);
        }

        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        public struct Enumerator : IEnumerator<Entity>
        {
            private EntitiesGroup _group;

            public Entity Current => _group.GetEntity(_index);
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