namespace DesertImage.ECS
{
    public static unsafe class WorldExtensions
    {
        public static Entity GetNewEntity(this in World world)
        {
            return Entities.GetNew(world.Ptr);
        }

        public static void Add<T>(this in World world, ExecutionType type = ExecutionType.MultiThread)
            where T : unmanaged, ISystem
        {
            Systems.Add<T>(world, type);
        }

        public static void AddFeature<T>(this in World world) where T : unmanaged, IFeature => new T().Link(world);

        public static void Remove<T>(this in World world) where T : unmanaged, ISystem
        {
            Systems.Remove<T>(world.SystemsState);
        }

        public static void Contains<T>(this in World world) where T : unmanaged, ISystem
        {
            Systems.Contains<T>(world.SystemsState);
        }

        public static void Tick(this in World world, float deltaTime)
        {
            Systems.Execute(Worlds.GetPtr(world.Id), deltaTime);
        }

        public static World GetWorldWithThisId(this ushort id)
        {
            return Worlds.Get(id);
        }
    }
}