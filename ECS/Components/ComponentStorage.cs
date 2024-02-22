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
        private UnsafeHashSet<uint> _hashes;
        private UnsafeArray<bool>* _entityComponents;
        private int* _lockIndexes;

        private long _size;
        private int _capacity;
        private int _entitiesCapacity;

        private long _lastOffset;

        private readonly Allocator _allocator;

        public ComponentStorage(int componentsCapacity, int entitiesCapacity)
        {
            _allocator = Allocator.Persistent;

            //TODO: fix size
            _data = MemoryUtility.AllocateClear<byte>(componentsCapacity, _allocator);

            var longSize = MemoryUtility.SizeOf<long>();
            var fullLongSize = longSize * entitiesCapacity;

            _offsets = MemoryUtility.AllocateClear<long>(fullLongSize, _allocator);
            _sizes = MemoryUtility.AllocateClear<long>(fullLongSize, _allocator);
            _lockIndexes = MemoryUtility.AllocateClear<int>(entitiesCapacity * MemoryUtility.SizeOf<int>(), _allocator);
            _hashes = new UnsafeHashSet<uint>(entitiesCapacity, _allocator);

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

            _lockIndexes[componentIndex].Lock();
            {
                var isNewId = !ContainsId(componentId);

                var elementSize = MemoryUtility.SizeOf<T>();
                var subArraySize = elementSize * _entitiesCapacity;

                var idOffset = isNewId ? _offsets[componentIndex] : 0;

                var entityIndex = (int)entityId;

                if (isNewId)
                {
                    var previousOffset = _lastOffset;
                    idOffset = previousOffset < 0 ? 0 : _lastOffset;

                    _lastOffset = idOffset + subArraySize;
                    _hashes.Add(componentId);

                    if (_entityComponents[entityIndex].IsNull)
                    {
                        var components = new UnsafeArray<bool>(_capacity, true, _allocator);
                        components[componentIndex] = true;
                        _entityComponents[entityIndex] = components;

                        var test = _entityComponents[entityIndex];
                    }
                    else
                    {
                        _entityComponents[entityIndex][componentIndex] = true;
                    }

                    _offsets[componentIndex] = idOffset;
                    _sizes[componentIndex] = elementSize;
                }

                if (idOffset >= _size)
                {
                    _lockIndexes[componentIndex].Unlock();
#if DEBUG
                    throw new Exception("out of array memory");
#endif
                    return;
                }

                UnsafeUtility.WriteArrayElement(_data + idOffset, entityIndex, data);
            }
            _lockIndexes[componentIndex].Unlock();
        }

        public void ClearEntityComponents(uint entityId)
        {
            var entityComponents = _entityComponents[(int)entityId];
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

        public ref T Read<T>(uint componentId, uint index) where T : unmanaged
        {
            var size = MemoryUtility.SizeOf<T>();
            var offset = _offsets[(int)componentId];

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
            var components = _entityComponents[(int)entityId];
            return !components.IsNull && components[(int)componentId];
        }

        public void Dispose()
        {
            UnsafeUtility.Free(_data, _allocator);
            MemoryUtility.Free(_offsets, _allocator);
            MemoryUtility.Free(_lockIndexes, _allocator);

            _hashes.Dispose();

            _data = null;
        }
    }
}