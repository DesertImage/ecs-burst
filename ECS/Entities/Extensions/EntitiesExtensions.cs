namespace DesertImage.ECS
{
    public static unsafe class EntitiesExtensions
    {
        public static void Destroy(this in Entity entity) => entity.DestroyEntity();

        public static bool IsAlive(this in Entity entity) => entity.IsAliveFlag == 1;
    }
}