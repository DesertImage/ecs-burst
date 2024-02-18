namespace DesertImage.ECS
{
    public static unsafe class EntitiesExtensions
    {
        public static T GetEntity<T>(this Entity entity) where T : unmanaged
        {
            return Components.Get<T>(entity, entity.GetWorld().State);
        }

        public static void Destroy(this Entity entity) => Entities.DestroyEntity(ref entity, entity.GetWorld().State);

        public static bool IsAlive(this Entity entity) => entity.IsAliveFlag == 1;

        public static World GetWorld(this Entity entity) => entity.WorldId.GetWorldWithThisId();
    }
}