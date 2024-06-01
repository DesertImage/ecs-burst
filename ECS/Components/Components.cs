using System;
using DesertImage.Collections;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public static unsafe class Components
    {
        public static void Remove<T>(in Entity entity, WorldState* state) where T : unmanaged
        {
            if (!Has<T>(entity, state))
            {
#if DEBUG_MODE
                throw new Exception($"Entity {entity} has not {typeof(T).Name}");
#else
                return;
#endif
            }

            var entityId = entity.Id;
            var componentId = ComponentTools.GetComponentId<T>();

            state->Components.Clear(entityId, componentId);

            if (!state->ComponentAllocations.TryGetValue(entityId, out var componentsSparseSet)) return;
            if (!componentsSparseSet.TryGetValue(componentId, out var ptrList)) return;

            foreach (var ptr in ptrList)
            {
                state->MemoryAllocator.Free(ptr);
            }
        }

        public static void Replace<T>(in Entity entity, WorldState* state) where T : unmanaged
        {
            Replace(entity, state, new T());
        }

        public static void Replace<T>(in Entity entity, WorldState* state, T component) where T : unmanaged
        {
#if DEBUG_MODE
            Entities.ThrowIfNotAlive(entity);
#endif
            state->Components.Set(entity.Id, component);
        }

        public static bool Has<T>(in Entity entity, WorldState* state) where T : unmanaged
        {
#if DEBUG_MODE
            Entities.ThrowIfNotAlive(entity);
#endif
            return state->Components.Contains<T>(entity.Id);
        }

        public static bool Has(in Entity entity, WorldState* state, uint componentId)
        {
            return state->Components.Contains(entity.Id, componentId);
        }

        public static ref T Get<T>(in Entity entity, WorldState* state) where T : unmanaged
        {
#if DEBUG_MODE
            Entities.ThrowIfNotAlive(entity);
#endif
            var componentId = ComponentTools.GetComponentIdFast<T>();
#if DEBUG_MODE
            if (!state->Components.Contains(entity.Id, componentId))
            {
                throw new Exception($"Entity: {entity.Id} has not {typeof(T)} ({componentId})");
            }
#endif
            return ref state->Components.Get<T>(entity.Id, componentId);
        }

        public static T Read<T>(in Entity entity, WorldState* state) where T : unmanaged
        {
#if DEBUG_MODE
            Entities.ThrowIfNotAlive(entity);
#endif
            var componentId = ComponentTools.GetComponentIdFast<T>();
#if DEBUG_MODE
            if (!state->Components.Contains(entity.Id, componentId))
            {
                throw new Exception($"Entity: {entity.Id} has not {typeof(T)} ({componentId})");
            }
#endif
            return state->Components.Read<T>(entity.Id, componentId);
        }

        public static void ReplaceStatic<T>(WorldState* state, T component) where T : unmanaged
        {
            var componentId = ComponentTools.GetComponentId<T>();

            if (state->StaticComponents.Contains(componentId))
            {
                var ptr = state->StaticComponents[componentId];
                UnsafeUtility.CopyStructureToPtr(ref component, (void*)ptr);
            }
            else
            {
                var componentPTr = (IntPtr)MemoryUtility.AllocateInstance(in component);
                state->StaticComponents.Set(componentId, componentPTr);
            }
        }

        public static T ReadStatic<T>(WorldState* state) where T : unmanaged
        {
            var componentId = ComponentTools.GetComponentIdFast<T>();
#if DEBUG_MODE
            if (!state->StaticComponents.Contains(componentId))
            {
                throw new Exception($"hasn't static component {typeof(T)}");
            }
#endif
            return *(T*)state->StaticComponents[componentId];
        }

        public static ref T GetStatic<T>(WorldState* state) where T : unmanaged
        {
            var componentId = ComponentTools.GetComponentIdFast<T>();
#if DEBUG_MODE
            if (!state->StaticComponents.Contains(componentId))
            {
                throw new Exception($"hasn't static component {typeof(T)}");
            }
#endif
            return ref *(T*)state->StaticComponents[componentId];
        }

        public static BufferArray<T> CreateBufferArray<T>(uint entityId, uint componentId, int length,
            WorldState* state)
            where T : unmanaged
        {
            var buffer = new BufferArray<T>(length, state);
            ref var allocations = ref state->ComponentAllocations;

            if (!allocations.TryGetValue(entityId, out var componentBindings))
            {
                componentBindings = new UnsafeUintSparseSet<UnsafeList<Ptr>>(ECSSettings.EntitiesCapacity);
            }

            if (!componentBindings.TryGetValue(componentId, out var ptrArray))
            {
                var newPtrList = new UnsafeList<Ptr>(3, Allocator.Persistent);
                componentBindings.Set(componentId, newPtrList);
                ptrArray = newPtrList;
            }

            ptrArray.Add(buffer.GetPtr());

            allocations.Set(entityId, componentBindings);

            return buffer;
        }

        public static BufferList<T> CreateBufferList<T>(uint entityId, uint componentId, int capacity,
            WorldState* state)
            where T : unmanaged
        {
            var buffer = new BufferList<T>(capacity, state);
            ref var allocations = ref state->ComponentAllocations;

            if (!allocations.TryGetValue(entityId, out var componentBindings))
            {
                componentBindings = new UnsafeUintSparseSet<UnsafeList<Ptr>>(ECSSettings.EntitiesCapacity);
            }

            if (!componentBindings.TryGetValue(componentId, out var ptrList))
            {
                var newPtrList = new UnsafeList<Ptr>(3, Allocator.Persistent);
                componentBindings.Set(componentId, newPtrList);
                ptrList = newPtrList;
            }

            ptrList.Add(buffer.GetPtr());

            allocations.Set(entityId, componentBindings);

            return buffer;
        }

        public static BufferUintSparseSet<T> CreateBufferSparseList<T>(uint entityId, uint componentId, int capacity,
            WorldState* state)
            where T : unmanaged
        {
            var buffer = new BufferUintSparseSet<T>(capacity, capacity, ref state->MemoryAllocator);
            ref var allocations = ref state->ComponentAllocations;

            if (!allocations.TryGetValue(entityId, out var componentBindings))
            {
                componentBindings = new UnsafeUintSparseSet<UnsafeList<Ptr>>(ECSSettings.EntitiesCapacity);
            }

            if (!componentBindings.TryGetValue(componentId, out var ptrList))
            {
                var newPtrList = new UnsafeList<Ptr>(3, Allocator.Persistent);
                componentBindings.Set(componentId, newPtrList);
                ptrList = newPtrList;
            }

            ptrList.Add(buffer.GetDensePtr());
            ptrList.Add(buffer.GetSparsePtr());
            ptrList.Add(buffer.GetKeysPtr());

            componentBindings.Set(componentId, ptrList);
            allocations.Set(entityId, componentBindings);

            return buffer;
        }

        public static BufferStack<T> CreateBufferStack<T>(uint entityId, uint componentId, int capacity,
            WorldState* state)
            where T : unmanaged
        {
            var buffer = new BufferStack<T>(capacity, state);
            ref var allocations = ref state->ComponentAllocations;

            if (!allocations.TryGetValue(entityId, out var componentBindings))
            {
                componentBindings = new UnsafeUintSparseSet<UnsafeList<Ptr>>(ECSSettings.EntitiesCapacity);
            }

            if (!componentBindings.TryGetValue(componentId, out var ptrQueue))
            {
                var newPtrList = new UnsafeList<Ptr>(3, Allocator.Persistent);
                componentBindings.Set(componentId, newPtrList);
                ptrQueue = newPtrList;
            }

            ptrQueue.Add(buffer.GetPtr());

            allocations.Set(entityId, componentBindings);

            return buffer;
        }

        public static void OnEntityDestroyed(in Entity entity, WorldState* state)
        {
            var entityId = entity.Id;

            state->Components.ClearAll(entityId);

            if (!state->ComponentAllocations.TryGetValue(entityId, out var allocations)) return;

            foreach (var ptrList in allocations)
            {
                foreach (var ptr in ptrList)
                {
                    state->MemoryAllocator.Free(ptr);
                }
            }
        }
    }
}