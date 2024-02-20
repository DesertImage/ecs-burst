using System;

namespace DesertImage.ECS
{
    public static unsafe class Components
    {
        public static void Remove<T>(in Entity entity, WorldState* state) where T : unmanaged
        {
            if (!Has<T>(entity, state))
            {
#if DEBUG
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
            Entities.ThrowIfNotAlive(entity);
            state->Components.Write(entity.Id, component);
        }

        public static bool Has<T>(in Entity entity, WorldState* state) where T : unmanaged
        {
            Entities.ThrowIfNotAlive(entity);
            return state->Components.Contains<T>(entity.Id);
        }

        public static bool Has(in Entity entity, WorldState* state, uint componentId)
        {
            return state->Components.Contains(entity.Id, componentId);
        }

        public static ref T Get<T>(in Entity entity, WorldState* state) where T : unmanaged
        {
            Entities.ThrowIfNotAlive(entity);

            var componentId = ComponentTools.GetComponentId<T>();
            return ref state->Components.Read<T>(componentId, entity.Id);
        }

        public static void ReplaceStatic<T>(in Entity entity, WorldState* state, T component) where T : unmanaged
        {
            Entities.ThrowIfNotAlive(entity);

            var componentId = ComponentTools.GetComponentId<T>();

            var componentPTr = (IntPtr)MemoryUtility.Allocate(component);

            if (state->StaticComponents.Contains(componentId))
            {
                state->StaticComponents[componentId] = componentPTr;
            }
            else
            {
                state->StaticComponents.Add(componentId, componentPTr);
            }
        }

        public static ref T GetStatic<T>(in Entity entity, WorldState* state) where T : unmanaged
        {
            Entities.ThrowIfNotAlive(entity);

            var componentId = ComponentTools.GetComponentId<T>();
            return ref *(T*)state->StaticComponents[componentId];
        }

        public static void OnEntityDestroyed(in Entity entity, WorldState* state)
        {
            state->Components.ClearEntityComponents(entity.Id);
        }
    }
}