namespace DesertImage.ECS
{
    public static class EntitiesExtensions
    {
        public static void Destroy(this Entity entity) => World.Current.DestroyEntity(entity.Id);
    }
}