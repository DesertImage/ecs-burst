namespace DesertImage.ECS
{
    public static unsafe partial class ComponentsExtensions
    {
        public static ref T Get<T>(this in Entity entity) where T : unmanaged
        {
            return ref Components.Get<T>(entity, entity.World->State);
        }

        public static T Read<T>(this in Entity entity) where T : unmanaged
        {
            return Components.Read<T>(entity, entity.World->State);
        }

        public static void Replace<T>(this in Entity entity, in T component) where T : unmanaged
        {
            var worldState = entity.World->State;
            Components.Replace(entity, worldState, component);
            Groups.OnEntityComponentAdded(entity, worldState, ComponentTools.GetComponentId<T>());
        }

        public static void Replace<T>(this in Entity entity) where T : unmanaged
        {
            var worldState = entity.World->State;
            Components.Replace<T>(entity, worldState);
            Groups.OnEntityComponentAdded(entity, worldState, ComponentTools.GetComponentId<T>());
        }

        public static void Remove<T>(this in Entity entity, bool dontDestroyOnZeroComponents = false) where T : unmanaged
        {
            var worldState = entity.World->State;
            Components.Remove<T>(entity, worldState, dontDestroyOnZeroComponents, out var isDestroyed);

            if (isDestroyed) return;

            Groups.OnEntityComponentRemoved(entity, worldState, ComponentTools.GetComponentIdFast<T>());
        }

        public static bool Has<T>(this in Entity entity) where T : unmanaged
        {
            return Components.Has<T>(entity, entity.World->State);
        }

        public static bool Has(this in Entity entity, uint componentId)
        {
            return Components.Has(entity, entity.World->State, componentId);
        }

        public static void ReplaceStatic<T>(this in Entity entity, T instance) where T : unmanaged
        {
            Components.ReplaceStatic(entity.World->State, instance);
        }

        public static ref T GetStatic<T>(this in Entity entity) where T : unmanaged
        {
#if DEBUG_MODE
            Entities.ThrowIfNotAlive(entity);
#endif
            return ref Components.GetStatic<T>(entity.World->State);
        }
    }
}