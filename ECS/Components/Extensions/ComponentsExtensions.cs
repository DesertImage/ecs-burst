namespace DesertImage.ECS
{
    public static unsafe partial class ComponentsExtensions
    {
        public static ref T Get<T>(this in Entity entity) where T : unmanaged
        {
            return ref Components.Get<T>(entity, entity.WorldId.GetWorldWithThisId().State);
        }
        
        public static T Read<T>(this in Entity entity) where T : unmanaged
        {
            return Components.Read<T>(entity, entity.WorldId.GetWorldWithThisId().State);
        }

        public static void Replace<T>(this in Entity entity, in T component) where T : unmanaged
        {
            var worldState = entity.GetWorld().State;
            Components.Replace(entity, worldState, component);
            Groups.OnEntityComponentAdded(entity, worldState, ComponentTools.GetComponentId<T>());
        }

        public static void Replace<T>(this in Entity entity) where T : unmanaged
        {
            var worldState = entity.GetWorld().State;
            Components.Replace<T>(entity, worldState);
            Groups.OnEntityComponentAdded(entity, worldState, ComponentTools.GetComponentId<T>());
        }

        public static void Remove<T>(this in Entity entity) where T : unmanaged
        {
            var worldState = entity.GetWorld().State;
            Components.Remove<T>(entity, worldState);
            Groups.OnEntityComponentRemoved(entity, worldState, ComponentTools.GetComponentId<T>());
        }

        public static bool Has<T>(this in Entity entity) where T : unmanaged
        {
            return Components.Has<T>(entity, entity.GetWorld().State);
        }

        internal static bool Has(this in Entity entity, uint componentId)
        {
            return Components.Has(entity, entity.GetWorld().State, componentId);
        }

        public static void ReplaceStatic<T>(this in Entity entity, T instance) where T : unmanaged
        {
            Components.ReplaceStatic(entity, entity.GetWorld().State, instance);
        }

        public static ref T GetStatic<T>(this in Entity entity) where T : unmanaged
        {
            return ref Components.GetStatic<T>(entity, entity.WorldId.GetWorldWithThisId().State);
        }
    }
}