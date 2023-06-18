namespace DesertImage.ECS
{
    public static class EntitiesExtensions
    {
        public static bool IsAlive(this Entity entity) => World.Current.IsEntityAlive(entity.Id);
        public static void Destroy(this Entity entity) => World.Current.DestroyEntity(entity.Id);
    }
}