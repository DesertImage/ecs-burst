namespace DesertImage.ECS
{
    public static partial class ComponentsExtensions
    {
        public static ref T Get<T>(this in Entity entity) where T : unmanaged
        {
            return ref Worlds.GetCurrent().GetComponent<T>(entity.Id);
        }

        public static void Replace<T>(this in Entity entity, in T component) where T : unmanaged
        {
            Worlds.GetCurrent().ReplaceComponent(entity.Id, component);
        }

        public static void Replace<T>(this in Entity entity) where T : unmanaged
        {
            Worlds.GetCurrent().ReplaceComponent(entity.Id, new T());
        }

        public static void Remove<T>(this in Entity entity) where T : unmanaged
        {
            Worlds.GetCurrent().RemoveComponent<T>(entity.Id);
        }

        public static bool Has<T>(this in Entity entity) where T : unmanaged => Worlds.GetCurrent().HasComponent<T>(entity.Id);
    }
}