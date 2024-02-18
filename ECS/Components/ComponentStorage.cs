using System;
using DesertImage.Collections;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public unsafe struct ComponentStorage : IDisposable
    {
        private struct ComponentData<T>
        {
            public T Data;
            public bool IsNotNull;
        }

        private byte* _data;
        private UnsafeArray<int> _offsets;
        private UnsafeArray<int> _sizes;
        private Collections.UnsafeHashSet<uint> _hashes;
        private UnsafeArray<UnsafeArray<bool>> _entityComponents;
        private UnsafeArray<int> _lockIndexes;

        private long _size;
        private int _entitiesCapacity;

        private int _lastOffset;

        private Allocator _allocator;

        public ComponentStorage(int componentsCapacity, int entitiesCapacity)
        {
            _allocator = Allocator.Persistent;

            // var size = UnsafeUtility.SizeOf<ComponentData<>>()
            //TODO: fix size
            _data = (byte*)UnsafeUtility.Malloc(componentsCapacity, 0, _allocator);
            UnsafeUtility.MemClear(_data, componentsCapacity);

            _offsets = new UnsafeArray<int>(entitiesCapacity, _allocator, 0);
            _sizes = new UnsafeArray<int>(entitiesCapacity, _allocator, -1);
            _lockIndexes = new UnsafeArray<int>(entitiesCapacity, _allocator, 0);
            _hashes = new Collections.UnsafeHashSet<uint>(entitiesCapacity, _allocator);
            _entityComponents = new UnsafeArray<UnsafeArray<bool>>
            (
                entitiesCapacity,
                _allocator,
                new UnsafeArray<bool>(componentsCapacity, Allocator.Persistent, false)
            );

            _lastOffset = -1;

            _size = componentsCapacity * entitiesCapacity;
            _entitiesCapacity = entitiesCapacity;
        }

        public void Write<T>(uint entityId, T data) where T : unmanaged
        {
            var componentId = ComponentTools.GetComponentId<T>();

            var componentIndex = (int)componentId;

            _lockIndexes.Get(componentIndex).Lock();
            {
                var isNewId = !ContainsId(componentId);

                var elementSize = MemoryUtility.GetSize<T>();
                var subArraySize = elementSize * _entitiesCapacity;

                var idOffset = isNewId ? _offsets[componentIndex] : 0;

                if (isNewId)
                {
                    var previousOffset = _lastOffset;
                    idOffset = previousOffset < 0 ? 0 : _lastOffset;

                    _lastOffset = idOffset + subArraySize;
                    _hashes.Add(componentId);

                    ref var components = ref _entityComponents.Get((int)entityId);
                    components[componentIndex] = true;

                    _offsets[componentIndex] = idOffset;
                    _sizes[componentIndex] = elementSize;
                }

                if (idOffset >= _size)
                {
                    _lockIndexes.Get(componentIndex).Unlock();
#if DEBUG
                    throw new Exception("out of array memory");
#endif
                    return;
                }

                MemoryUtility.Write(_data, idOffset, entityId, data, elementSize);
            }
            _lockIndexes.Get(componentIndex).Unlock();
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

        public void Clear(uint entityId, uint componentId)
        {
            var componentIndex = (int)componentId;

            _lockIndexes.Get(componentIndex).Lock();
            {
                if (!ContainsId(componentId))
                {
                    _lockIndexes.Get(componentIndex).Unlock();
                    throw new Exception("Entity");
                }

                var elementSize = _sizes[componentIndex];

                var idOffset = _offsets[componentIndex];
                if (idOffset >= _size)
                {
                    _lockIndexes.Get(componentIndex).Unlock();
#if DEBUG
                    throw new Exception("out of array memory");
#endif
                    return;
                }

                UnsafeUtility.MemClear(_data + idOffset + elementSize * entityId, elementSize);

                ref var components = ref _entityComponents.Get((int)entityId);
                components[componentIndex] = false;
            }
            _lockIndexes.Get(componentIndex).Unlock();
        }

        public ref T Read<T>(uint componentId, uint index) where T : unmanaged
        {
            var size = MemoryUtility.GetSize<T>();
            var offset = _offsets[(int)componentId];

            return ref MemoryUtility.Read<T>(_data, offset, index, size);
        }

        public bool ContainsId(uint id) => _hashes.Contains(id);

        public bool Contains<T>(uint entityId) where T : unmanaged
        {
            var id = ComponentTools.GetComponentId<T>();
            return Contains(entityId, id);
        }

        public bool Contains(uint entityId, uint componentId) => _entityComponents[(int)entityId][(int)componentId];

        public void Dispose()
        {
            UnsafeUtility.Free(_data, _allocator);
            _data = null;

            _offsets.Dispose();
            _hashes.Dispose();
            _lockIndexes.Dispose();
        }
    }
}