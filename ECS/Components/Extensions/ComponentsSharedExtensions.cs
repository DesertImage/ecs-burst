namespace DesertImage.ECS
{
    public static partial class ComponentsExtensions
    {
        public static ref T GetShared<T>(this in Entity entity) where T : struct, ISharedComponent
        {
            return ref Worlds.Current.GetSharedComponent<T>(entity.Id);
        }

        public static void ReplaceShared<T>(this in Entity entity, in T component) where T : struct, ISharedComponent
        {
            Worlds.Current.ReplaceSharedComponent(entity.Id, component);
        }

        public static void ReplaceShared<T>(this in Entity entity) where T : struct, ISharedComponent
        {
            Worlds.Current.ReplaceSharedComponent(entity.Id, new T());
        }

        public static void RemoveShared<T>(this in Entity entity) where T : struct, ISharedComponent
        {
            Worlds.Current.RemoveSharedComponent<T>(entity.Id);
        }

        public static bool HasShared<T>(this in Entity entity) where T : struct, ISharedComponent
        {
            return Worlds.Current.HasSharedComponent<T>(entity.Id);
        }
        
        public static ref T GetStatic<T>(this in Entity entity) where T : struct, IStaticComponent
        {
            return ref Worlds.Current.GetStaticComponent<T>(entity.Id);
        }

        public static void ReplaceStatic<T>(this in Entity entity, in T component) where T : struct, IStaticComponent
        {
            Worlds.Current.ReplaceStaticComponent(entity.Id, component);
        }

        public static void ReplaceStatic<T>(this in Entity entity) where T : struct, IStaticComponent
        {
            Worlds.Current.ReplaceStaticComponent(entity.Id, new T());
        }

        public static bool HasStatic<T>(this in Entity entity) where T : struct, IStaticComponent
        {
            return Worlds.Current.HasStaticComponent<T>(entity.Id);
        }
    }
}