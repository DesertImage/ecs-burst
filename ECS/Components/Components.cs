using System;
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

            state->Components.Clear<T>(entity.Id);
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
                throw new Exception($"Entity: {entity.Id} has not {typeof(T)}");
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
                throw new Exception($"Entity: {entity.Id} has not {typeof(T)}");
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

        public static ref T GetStatic<T>(WorldState* state) where T : unmanaged
        {
            var componentId = ComponentTools.GetComponentIdFast<T>();
            return ref *(T*)state->StaticComponents[componentId];
        }

        public static void OnEntityDestroyed(in Entity entity, WorldState* state)
        {
            state->Components.ClearAll(entity.Id);
        }
    }
}