namespace DesertImage.ECS
{
    public static class EntitiesExtensions
    {
        public static bool IsAlive(this Entity entity) => Worlds.Current.IsEntityAlive(entity.Id);
        public static void Destroy(this Entity entity) => Worlds.Current.DestroyEntity(entity.Id);
    }
}