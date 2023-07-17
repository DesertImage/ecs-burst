namespace DesertImage.ECS
{
    public static class ComponentsExtensions
    {
        public static ref T Get<T>(this in Entity entity) where T : struct
        {
            return ref Worlds.Current.GetComponent<T>(entity.Id);
        }

        public static void Replace<T>(this in Entity entity, in T component) where T : struct
        {
            Worlds.Current.ReplaceComponent(entity.Id, component);
        }

        public static void Replace<T>(this in Entity entity) where T : struct
        {
            Worlds.Current.ReplaceComponent(entity.Id, new T());
        }

        public static void Remove<T>(this in Entity entity) where T : struct
        {
            Worlds.Current.RemoveComponent<T>(entity.Id);
        }

        public static bool Has<T>(this in Entity entity) where T : struct => Worlds.Current.HasComponent<T>(entity.Id);
    }
}