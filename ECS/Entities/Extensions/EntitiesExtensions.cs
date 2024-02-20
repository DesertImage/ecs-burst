namespace DesertImage.ECS
{
    public static unsafe class EntitiesExtensions
    {
        public static T GetEntity<T>(this in Entity entity) where T : unmanaged
        {
            return Components.Get<T>(entity, entity.GetWorld().State);
        }

        public static void Destroy(this in Entity entity) => Entities.DestroyEntity(in entity, entity.GetWorld().State);

        public static bool IsAlive(this in Entity entity) => entity.IsAliveFlag == 1;

        public static World GetWorld(this in Entity entity) => entity.WorldId.GetWorldWithThisId();
    }
}