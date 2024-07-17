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
        private int _componentsCapacity;
        private readonly int _entitiesCapacity;

        private long _lastOffset;
        private int _count;

        private UnsafeNoCollisionHashSet<uint> _hashes;

        private readonly Allocator _allocator;

        public ComponentStorage(int componentsCapacity, int entitiesCapacity,
            Allocator allocator = Allocator.Persistent)
        {
            _size = componentsCapacity * MemoryUtility.SizeOf<UnsafeUintSparseSet<long>>();

            _data = MemoryUtility.AllocateClear<byte>(_size, allocator);
            _offsets = MemoryUtility.AllocateClearCapacity<long>(componentsCapacity, allocator);
            _hashes = new UnsafeNoCollisionHashSet<uint>(componentsCapacity, allocator);

            _componentsCapacity = componentsCapacity;
            _entitiesCapacity = entitiesCapacity;

            _lastOffset = 0;
            _count = 0;

            _allocator = allocator;
        }

        public void Set<T>(uint entityId, T data) where T : unmanaged
        {
            var componentId = ComponentTools.GetComponentId<T>();

            if (componentId >= _componentsCapacity)
            {
                var newCapacity = _componentsCapacity << 1;
                if (newCapacity <= componentId)
                {
                    newCapacity = (int)(componentId + 1);
                }

                _offsets = MemoryUtility.Resize(_offsets, _componentsCapacity, newCapacity);
                _componentsCapacity = newCapacity;
            }

            var offset = _offsets[componentId];

            var isNewStorage = _count == 0 || !_hashes.Contains(componentId);

            if (isNewStorage)
            {
                ref var sparseSet = ref InitComponent(componentId, MemoryUtility.SizeOf<T>());
                sparseSet.Add(entityId, data);
            }
            else
            {
                //TODO: fix for .Udpate(entityId, data);
                ((UnsafeUintUnknownTypeSparseSet*)(_data + offset))->AddOrSet(entityId, data);
            }
        }

        private ref UnsafeUintUnknownTypeSparseSet InitComponent(uint componentId, long componentSize)
        {
            var sparseSet = new UnsafeUintUnknownTypeSparseSet
            (
                _entitiesCapacity / 2,
                _entitiesCapacity, componentSize
            );

            var offset = _offsets[componentId] = _lastOffset;
            _lastOffset += MemoryUtility.SizeOf<UnsafeUintUnknownTypeSparseSet>();

            if (_lastOffset >= _size)
            {
                var newSize = _size << 1;
                _data = MemoryUtility.Resize(_data, _size, newSize, _allocator);
                _size = newSize;
            }

            _count++;

            _hashes.Add(componentId);
            *(UnsafeUintUnknownTypeSparseSet*)(_data + offset) = sparseSet;

            return ref *(UnsafeUintUnknownTypeSparseSet*)(_data + offset);
        }

        public T Read<T>(uint entityId, uint componentId) where T : unmanaged
        {
            return ((UnsafeUintUnknownTypeSparseSet*)(_data + _offsets[componentId]))->Read<T>(entityId);
        }

        public ref T Get<T>(uint entityId, uint componentId) where T : unmanaged
        {
            return ref ((UnsafeUintUnknownTypeSparseSet*)(_data + _offsets[componentId]))->Get<T>(entityId);
        }

        public UnsafeUintReadOnlySparseSet<T> GetComponents<T>() where T : unmanaged
        {
            var componentId = ComponentTools.GetComponentId<T>();

            return !ContainsKey(componentId)
                ? InitComponent(componentId, MemoryUtility.SizeOf<T>()).ToReadOnly<T>()
                : ((UnsafeUintUnknownTypeSparseSet*)(_data + _offsets[componentId]))->ToReadOnly<T>();
        }

        public void* GetPtr<T>(uint entityId) where T : unmanaged
        {
            var componentId = ComponentTools.GetComponentId<T>();
            return GetPtr(entityId, componentId);
        }

        public void* GetPtr(uint entityId, uint componentId)
        {
            return ((UnsafeUintUnknownTypeSparseSet*)(_data + _offsets[componentId]))->GetPtr(entityId);
        }

        // public ref UnsafeUintUnknownTypeSparseSet GetSparseSet<T>() where T : unmanaged
        // {
        // return ref GetSparseSet(ComponentTools.GetComponentIdFast<T>());
        // }

        public ref UnsafeUintUnknownTypeSparseSet GetSparseSet(uint componentId)
        {
#if DEBUG_MODE
            if (!ContainsKey(componentId)) throw new Exception($"storage hasn't component: {componentId}");
#endif
            return ref *(UnsafeUintUnknownTypeSparseSet*)(_data + _offsets[componentId]);
        }

        public ref UnsafeUintUnknownTypeSparseSet GetSparseSetOrInitialize(uint componentId, long componentSize)
        {
            if (!ContainsKey(componentId)) return ref InitComponent(componentId, componentSize);
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

        public void Clear(uint entityId, uint componentId)
        {
            ((UnsafeUintUnknownTypeSparseSet*)(_data + _offsets[componentId]))->Remove(entityId);
        }

        public void ClearAll(uint entityId)
        {
            var found = 0;
            var index = 0;

            while (found < _count && _count < _componentsCapacity)
            {
                var offset = _offsets[++index];
                
                if(offset == 0 && index != 1) continue;
                
                found++;
                
                var unsafeUintUnknownTypeSparseSet = (UnsafeUintUnknownTypeSparseSet*)(_data + offset);
                unsafeUintUnknownTypeSparseSet->Remove(entityId);
            }
        }

        public void Dispose()
        {
            foreach (var componentId in _hashes)
            {
                var offset = _offsets[componentId];
                ((UnsafeUintUnknownTypeSparseSet*)(_data + offset))->Dispose();
            }

            MemoryUtility.Free(_data, _allocator);
            MemoryUtility.Free(_offsets, _allocator);
            _hashes.Dispose();

            _data = null;
            _offsets = null;
        }

        sealed class ComponentStorageNewDebugView
        {
            private ComponentStorage _data;

            public ComponentStorageNewDebugView(ComponentStorage data) => _data = data;

            public long[] Offsets => MemoryUtility.ToArray(_data._offsets, _data._componentsCapacity);
        }
    }
}