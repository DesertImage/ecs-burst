using System;
using DesertImage.Collections;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace DesertImage.ECS
{
    public unsafe struct ComponentStorage : IDisposable
    {
        private byte* _data;
        private long* _offsets;
        private long* _sizes;
        private UnsafeNoCollisionHashSet<uint> _hashes;

        private UnsafeArray<bool>* _entityComponents;

        //TODO: lock on one entity's component, not component for all entities
        private int* _lockIndexes;

        private long _size;
        private int _capacity;
        private int _entitiesCapacity;

        private long _lastOffset;

        private readonly Allocator _allocator;

        public ComponentStorage(int componentsCapacity, int entitiesCapacity)
        {
            _allocator = Allocator.Persistent;

            _data = MemoryUtility.AllocateClear<byte>(componentsCapacity * entitiesCapacity, _allocator);

            var longSize = MemoryUtility.SizeOf<long>();
            var fullLongSize = longSize * componentsCapacity;

            _offsets = MemoryUtility.AllocateClear<long>(fullLongSize, _allocator);
            _sizes = MemoryUtility.AllocateClear<long>(fullLongSize, _allocator);
            _lockIndexes = MemoryUtility.AllocateClear<int>(entitiesCapacity * MemoryUtility.SizeOf<int>(), _allocator);

            _hashes = new UnsafeNoCollisionHashSet<uint>(componentsCapacity, _allocator);
            _entityComponents = MemoryUtility.AllocateClear<UnsafeArray<bool>>
            (
                entitiesCapacity * MemoryUtility.SizeOf<UnsafeArray<bool>>(),
                _allocator
            );

            _lastOffset = -1;

            _size = componentsCapacity * entitiesCapacity;
            _capacity = componentsCapacity;
            _entitiesCapacity = entitiesCapacity;
        }

        public void Write<T>(uint entityId, T data) where T : unmanaged
        {
            var componentId = ComponentTools.GetComponentId<T>();
            var componentIndex = (int)componentId;

            var isOutOfEntityCapacity = entityId >= _entitiesCapacity;
            var isOutOfComponentsCapacity = componentIndex >= _capacity;

            if (isOutOfComponentsCapacity || isOutOfEntityCapacity)
            {
                Resize
                (
                    isOutOfEntityCapacity ? _entitiesCapacity << 1 : _entitiesCapacity,
                    isOutOfComponentsCapacity ? _capacity << 1 : _capacity
                );
            }

#if DEBUG
            if (componentIndex >= _capacity)
            {
                throw new IndexOutOfRangeException();
            }
#endif

            _lockIndexes[componentIndex].Lock();
            {
                var isNewId = !ContainsId(componentId);

                var elementSize = MemoryUtility.SizeOf<T>();
                var subArraySize = elementSize * _entitiesCapacity;

                var idOffset = isNewId ? _offsets[componentIndex] : 0;

                var entityIndex = (int)entityId;

#if DEBUG
                if (entityIndex >= _entitiesCapacity)
                {
                    throw new IndexOutOfRangeException();
                }
#endif

                if (isNewId)
                {
                    var previousOffset = _lastOffset;
                    idOffset = previousOffset < 0 ? 0 : _lastOffset;

                    _lastOffset = idOffset + subArraySize;

                    _hashes.Add(componentId);

                    _offsets[componentIndex] = idOffset;
                    _sizes[componentIndex] = elementSize;
                }
#if DEBUG
                if (idOffset >= _size)
                {
                    _lockIndexes[componentIndex].Unlock();
                    throw new Exception("out of array memory");
                }
#endif
                if (!_entityComponents[entityIndex].IsNotNull)
                {
                    var components = new UnsafeArray<bool>(_capacity, true, _allocator);
                    components[componentIndex] = true;
                    _entityComponents[entityIndex] = components;
                }
                else
                {
                    _entityComponents[entityIndex][componentIndex] = true;
                }

#if UNITY_EDITOR
                ComponentsDebug.Add(entityId, data);
#endif
                UnsafeUtility.WriteArrayElement(_data + idOffset, entityIndex, data);
            }
            _lockIndexes[componentIndex].Unlock();
        }

        public void ClearEntityComponents(uint entityId)
        {
#if UNITY_EDITOR
            ComponentsDebug.RemoveAll(entityId);
#endif
#if DEBUG
            if (entityId >= _entitiesCapacity)
            {
                throw new IndexOutOfRangeException();
            }
#endif
            var entityComponents = _entityComponents[(int)entityId];

            if (!entityComponents.IsNotNull) return;

            for (var i = 0; i < entityComponents.Length; i++)
            {
                var isHas = entityComponents[i];

                if (!isHas) continue;

                Clear(entityId, (uint)i);
            }
        }

        public void Clear<T>(uint entityId) where T : unmanaged => Clear(entityId, ComponentTools.GetComponentId<T>());

        private void Clear(uint entityId, uint componentId)
        {
            var componentIndex = (int)componentId;

#if UNITY_EDITOR
            ComponentsDebug.Remove(entityId, componentId);
#endif
#if DEBUG
            if (componentIndex >= _capacity)
            {
                throw new IndexOutOfRangeException();
            }
#endif

            _lockIndexes[componentIndex].Lock();
            {
                if (!ContainsId(componentId))
                {
                    _lockIndexes[componentIndex].Unlock();
                    throw new Exception("Entity");
                }

                var elementSize = _sizes[componentIndex];

                var idOffset = _offsets[componentIndex];
                if (idOffset >= _size)
                {
                    _lockIndexes[componentIndex].Unlock();
#if DEBUG
                    throw new Exception("out of array memory");
#endif
                    return;
                }

                UnsafeUtility.MemClear(_data + idOffset + elementSize * entityId, elementSize);

                _entityComponents[(int)entityId][componentIndex] = false;
            }
            _lockIndexes[componentIndex].Unlock();
        }

        public void Resize(int newEntitiesCapacity, int newComponentsCapacity)
        {
            var longSize = MemoryUtility.SizeOf<long>();

            var oldEntitiesCapacity = _entitiesCapacity;
            var oldSize = _size;

            var oldFullLongSize = longSize * _capacity;
            var newFullLongSize = longSize * newComponentsCapacity;

            var newSize = newEntitiesCapacity * newComponentsCapacity;

            MemoryUtility.Resize(ref _data, oldSize, newSize);
            MemoryUtility.Resize(ref _offsets, oldFullLongSize, newFullLongSize);
            MemoryUtility.Resize(ref _sizes, oldFullLongSize, newFullLongSize);
            MemoryUtility.Resize(ref _lockIndexes, oldEntitiesCapacity, newEntitiesCapacity);
            MemoryUtility.Resize(ref _entityComponents, oldEntitiesCapacity, newEntitiesCapacity);

            _size = newSize;
            _capacity = newComponentsCapacity;
            _entitiesCapacity = newEntitiesCapacity;
        }

        public T Read<T>(uint componentId, uint index) where T : unmanaged
        {
            var offset = _offsets[(int)componentId];
#if DEBUG
            if (index >= _entitiesCapacity)
            {
                throw new IndexOutOfRangeException();
            }
#endif
            return UnsafeUtility.ReadArrayElement<T>(_data + offset, (int)index);
        }

        public ref T Get<T>(uint componentId, uint index) where T : unmanaged
        {
            var offset = _offsets[(int)componentId];
#if DEBUG
            if (index >= _entitiesCapacity)
            {
                throw new IndexOutOfRangeException();
            }
#endif
            return ref UnsafeUtility.ArrayElementAsRef<T>(_data + offset, (int)index);
        }

        public bool ContainsId(uint id) => _hashes.Contains(id);

        public bool Contains<T>(uint entityId) where T : unmanaged
        {
            var id = ComponentTools.GetComponentId<T>();
            return Contains(entityId, id);
        }

        public bool Contains(uint entityId, uint componentId)
        {
#if DEBUG
            if (entityId >= _entitiesCapacity)
            {
                throw new IndexOutOfRangeException();
            }

            if (componentId >= _capacity)
            {
                throw new IndexOutOfRangeException();
            }
#endif
            var components = _entityComponents[(int)entityId];
            return components.IsNotNull && components[(int)componentId];
        }

        public void Dispose()
        {
            UnsafeUtility.Free(_data, _allocator);
            MemoryUtility.Free(_offsets, _allocator);
            MemoryUtility.Free(_sizes, _allocator);
            MemoryUtility.Free(_lockIndexes, _allocator);

            _hashes.Dispose();

            _data = null;
        }
    }
}