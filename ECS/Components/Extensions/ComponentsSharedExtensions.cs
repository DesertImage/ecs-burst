namespace DesertImage.ECS
{
    public static partial class ComponentsExtensions
    {
        public static ref T GetShared<T>(this in Entity entity) where T : unmanaged, ISharedComponent
        {
            return ref Worlds.GetCurrent().GetSharedComponent<T>(entity.Id);
        }

        public static void ReplaceShared<T>(this in Entity entity, in T component) where T : unmanaged, ISharedComponent
        {
            Worlds.GetCurrent().ReplaceSharedComponent(entity.Id, component);
        }

        public static void ReplaceShared<T>(this in Entity entity) where T : unmanaged, ISharedComponent
        {
            Worlds.GetCurrent().ReplaceSharedComponent(entity.Id, new T());
        }

        public static void RemoveShared<T>(this in Entity entity) where T : unmanaged, ISharedComponent
        {
            Worlds.GetCurrent().RemoveSharedComponent<T>(entity.Id);
        }

        public static bool HasShared<T>(this in Entity entity) where T : unmanaged, ISharedComponent
        {
            return Worlds.GetCurrent().HasSharedComponent<T>(entity.Id);
        }
        
        public static ref T GetStatic<T>(this in Entity entity) where T : unmanaged, IStaticComponent
        {
            return ref Worlds.GetCurrent().GetStaticComponent<T>(entity.Id);
        }

        public static void ReplaceStatic<T>(this in Entity entity, in T component) where T : unmanaged, IStaticComponent
        {
            Worlds.GetCurrent().ReplaceStaticComponent(entity.Id, component);
        }

        public static void ReplaceStatic<T>(this in Entity entity) where T : unmanaged, IStaticComponent
        {
            Worlds.GetCurrent().ReplaceStaticComponent(entity.Id, new T());
        }

        public static bool HasStatic<T>(this in Entity entity) where T : unmanaged, IStaticComponent
        {
            return Worlds.GetCurrent().HasStaticComponent<T>(entity.Id);
        }
    }
}