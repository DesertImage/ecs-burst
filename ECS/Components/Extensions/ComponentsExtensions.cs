namespace DesertImage.ECS
{
    public static class ComponentsExtensions
    {
        public static ref T Get<T>(this Entity entity) where T : struct => ref World.Current.GetComponent<T>(entity.Id);

        public static void Replace<T>(this Entity entity, T component) where T : struct
        {
            World.Current.ReplaceComponent(entity.Id, component);
        }
        
        public static void Replace<T>(this Entity entity) where T : struct
        {
            World.Current.ReplaceComponent(entity.Id, new T());
        }

        public static void Remove<T>(this Entity entity) where T : struct
        {
            World.Current.RemoveComponent<T>(entity.Id);
        }

        public static bool Has<T>(this Entity entity) where T : struct => World.Current.HasComponent<T>(entity.Id);
    }
}