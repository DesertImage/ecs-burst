using System;
using System.Collections.Generic;
using DesertImage.ECS;
using DesertImage.ECS;
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
        private int* _offsets;
        private UnsafeHashSet<int> _hashes;
        private UnsafeArray<int> _lockIndexes;

        private long _size;
        private int _capacity;

        private int _lastOffset;

        private Allocator _allocator;

        public ComponentStorage(long size, int capacity)
        {
            var intSize = capacity * UnsafeUtility.SizeOf<int>();

            _data = (byte*)UnsafeUtility.Malloc(size, 0, Allocator.Persistent);
            _offsets = (int*)UnsafeUtility.Malloc(intSize, 0, Allocator.Persistent);
            
            _lockIndexes = new UnsafeArray<int>(capacity, Allocator.Persistent, 0);
            _hashes = new UnsafeHashSet<int>(capacity, Allocator.Persistent);

            _lastOffset = -1;

            _allocator = Allocator.Persistent;

            _size = size;
            _capacity = capacity;
        }

        public ComponentStorage(byte* data, Allocator allocator, long size, int capacity)
        {
            var intSize = capacity * UnsafeUtility.SizeOf<int>();

            _data = data;
            _offsets = (int*)UnsafeUtility.Malloc(intSize, 0, Allocator.Persistent);
            
            _lockIndexes = new UnsafeArray<int>(capacity, Allocator.Persistent);
            _hashes = new UnsafeHashSet<int>(capacity, allocator);

            for (var i = 0; i < capacity; i++)
            {
                _lockIndexes[i] = 0;
            }

            _lastOffset = -1;

            _allocator = allocator;

            _size = size;
            _capacity = capacity;
        }

        public void Write<T>(int id, int entityId, T data) where T : unmanaged
        {
            _lockIndexes.Get(id).Lock();
            {
                var isNewId = !ContainsId(id);

                var elementSize = MemoryUtility.GetSize<ComponentData<T>>();
                var subArraySize = elementSize * _capacity;

                var idOffset = isNewId ? MemoryUtility.Read(_offsets, entityId) : 0;

                if (isNewId)
                {
                    var previousOffset = _lastOffset;
                    idOffset = previousOffset < 0 ? 0 : _lastOffset;

                    _lastOffset = idOffset + subArraySize;
                    _hashes.Add(id);

                    MemoryUtility.Write(_offsets, id, idOffset);
                }

                if (idOffset >= _size)
                {
#if DEBUG
                    throw new Exception("out of array memory");
#endif
                    return;
                }

                var componentData = new ComponentData<T> { Data = data, IsNotNull = true };
                MemoryUtility.Write(_data, idOffset, entityId, componentData, elementSize);
            }
            _lockIndexes.Get(id).Unlock();
        }

        public ref T Read<T>(int id, int index) where T : unmanaged
        {
            var size = MemoryUtility.GetSize<ComponentData<T>>();
            var offset = MemoryUtility.Read(_offsets, id);

            ref var componentData = ref MemoryUtility.Read<ComponentData<T>>(_data, offset, index, size);
            return ref componentData.Data;
        }

        private ComponentData<T> ReadComponentData<T>(int id, int index) where T : unmanaged
        {
            var size = MemoryUtility.GetSize<ComponentData<T>>();
            var offset = MemoryUtility.Read(_offsets, id);

            var data = MemoryUtility.Read<ComponentData<T>>(_data, offset, index, size);

            return data;
        }
        
        private bool IsPtrNull<T>(int id, int index) where T : unmanaged
        {
            var size = MemoryUtility.GetSize<ComponentData<T>>();
            var offset = MemoryUtility.Read(_offsets, id);

            return MemoryUtility.IsNull<ComponentData<T>>(_data, offset, index, size);
        }

        public bool ContainsId(int id) => _hashes.Contains(id);

        public bool Contains<T>(int entityId) where T : unmanaged
        {
            var id = ComponentTools.GetComponentId<T>();

            if(IsPtrNull<T>(id, entityId)) return false;

            var componentData = ReadComponentData<T>(id, entityId);
            return componentData.IsNotNull;
        }

        public void Dispose()
        {
            UnsafeUtility.Free(_data, _allocator);
            UnsafeUtility.Free(_offsets, _allocator);
            _hashes.Dispose();
        }
    }
}