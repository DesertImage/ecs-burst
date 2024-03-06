using System;
using System.Diagnostics;
using DesertImage.Collections;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    [DebuggerDisplay("Count = {_count}. Size = {_size}")]
    [DebuggerTypeProxy(typeof(ComponentStorageNewDebugView))]
    public unsafe struct ComponentStorage : IDisposable
    {
        private byte* _data;
        private long* _offsets;

        private long _size;
        private int _capacity;
        private readonly int _entitiesCapacity;

        private long _lastOffset;
        private int _count;

        private UnsafeUintHashSet _hashes;

        private readonly Allocator _allocator;

        public ComponentStorage(int componentsCapacity, int entityCapacity,
            Allocator allocator = Allocator.Persistent)
        {
            _size = componentsCapacity * MemoryUtility.SizeOf<UnsafeUintSparseSet<long>>();

            _data = MemoryUtility.AllocateClear<byte>(_size, allocator);
            _offsets = MemoryUtility.AllocateClearCapacity<long>(componentsCapacity, allocator);
            _hashes = new UnsafeUintHashSet(componentsCapacity, allocator);

            _capacity = componentsCapacity;
            _entitiesCapacity = entityCapacity;

            _lastOffset = 0;
            _count = 0;

            _allocator = allocator;
        }

        public void Set<T>(uint entityId, T data) where T : unmanaged
        {
            var componentId = ComponentTools.GetComponentId<T>();

            var isOutOfCapacity = componentId >= _capacity;

            if (isOutOfCapacity)
            {
                var newCapacity = _capacity << 1;
                if (newCapacity <= componentId)
                {
                    newCapacity = (int)(componentId + 1);
                }

                MemoryUtility.Resize(ref _offsets, _capacity, newCapacity);
                _capacity = newCapacity;
            }

            var offset = _offsets[componentId];

            var isNew = _count == 0 || !_hashes.Contains(componentId);

            if (isNew)
            {
                var sparseSet = new UnsafeUintUnknownTypeSparseSet
                (
                    _entitiesCapacity / 2,
                    _entitiesCapacity, MemoryUtility.SizeOf<T>()
                );

                sparseSet.Set(entityId, data);

                _offsets[componentId] = offset = _lastOffset;
                _lastOffset += MemoryUtility.SizeOf<UnsafeUintUnknownTypeSparseSet>();

                if (_lastOffset >= _size)
                {
                    var newSize = _size << 1;
                    MemoryUtility.Resize(ref _data, _size, newSize);
                    _size = newSize;
                }

                _count++;

                _hashes.Add(componentId);
                *(UnsafeUintUnknownTypeSparseSet*)(_data + offset) = sparseSet;
            }
            else
            {
                ((UnsafeUintUnknownTypeSparseSet*)(_data + offset))->Set(entityId, data);
            }
        }

        public T Read<T>(uint entityId, uint componentId) where T : unmanaged
        {
            return ((UnsafeUintUnknownTypeSparseSet*)(_data + _offsets[componentId]))->Read<T>(entityId);
        }

        public ref T Get<T>(uint entityId, uint componentId) where T : unmanaged
        {
            return ref ((UnsafeUintUnknownTypeSparseSet*)(_data + _offsets[componentId]))->Get<T>(entityId);
        }

        public void* GetPtr(uint entityId, uint componentId)
        {
            return ((UnsafeUintUnknownTypeSparseSet*)(_data + _offsets[componentId]))->GetPtr(entityId);
        }

        public ref UnsafeUintUnknownTypeSparseSet ReadSparsSet<T>() where T : unmanaged
        {
            return ref ReadSparsSet(ComponentTools.GetComponentIdFast<T>());
        }

        public ref UnsafeUintUnknownTypeSparseSet ReadSparsSet(uint componentId)
        {
            return ref *(UnsafeUintUnknownTypeSparseSet*)(_data + _offsets[componentId]);
        }

        public bool ContainsKey(uint componentId) => _hashes.Contains(componentId);

        public bool Contains<T>(uint entityId) where T : unmanaged
        {
            return Contains(entityId, ComponentTools.GetComponentId<T>());
        }

        public bool Contains(uint entityId, uint componentId)
        {
            return _hashes.Contains(componentId) &&
                   ((UnsafeUintUnknownTypeSparseSet*)(_data + _offsets[componentId]))->Contains(entityId);
        }

        public void Clear<T>(uint entityId) where T : unmanaged
        {
            var componentId = ComponentTools.GetComponentIdFast<T>();
            ((UnsafeUintUnknownTypeSparseSet*)(_data + _offsets[componentId]))->Remove(entityId);
        }

        public void ClearAll(uint entityId)
        {
            for (var i = 0; i < _count; i++)
            {
                var offset = _offsets[i];

                if (i > 0 && offset == 0) continue;

                ((UnsafeUintUnknownTypeSparseSet*)(_data + offset))->Remove(entityId);
            }
        }

        public void Dispose()
        {
            for (var i = 0; i < _capacity; i++)
            {
                var offset = _offsets[i];

                if (i > 0 && offset == 0) continue;

                ((UnsafeUintUnknownTypeSparseSet*)(_data + offset))->Dispose();
            }

            UnsafeUtility.Free(_data, _allocator);
            MemoryUtility.Free(_offsets, _allocator);
            _hashes.Dispose();

            _data = null;
            _offsets = null;
        }

        sealed class ComponentStorageNewDebugView
        {
            private ComponentStorage _data;

            public ComponentStorageNewDebugView(ComponentStorage data) => _data = data;

            public long[] Offsets => MemoryUtility.ToArray(_data._offsets, _data._capacity);
        }
    }
}