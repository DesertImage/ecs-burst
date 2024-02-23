namespace DesertImage.ECS
{
    public static unsafe class WorldExtensions
    {
        public static Entity GetNewEntity(this World world)
        {
            return Entities.GetNew(world);
        }

        public static void Add<T>(this World world, ExecutionType type = ExecutionType.MultiThread)
            where T : unmanaged, ISystem
        {
            Systems.Add<T>(world, type);
        }

        public static void Remove<T>(this World world) where T : unmanaged, ISystem
        {
            Systems.Remove<T>(world.SystemsState);
        }

        public static void Contains<T>(this World world) where T : unmanaged, ISystem
        {
            Systems.Contains<T>(world.SystemsState);
        }

        public static void Tick(this World world, float deltaTime)
        {
            Systems.Execute(Worlds.GetPtr(world.Id), deltaTime);
        }

        public static EntitiesGroup GetGroup(this World world, Matcher matcher)
        {
            return Groups.GetGroup(matcher, world);
        }

        public static World GetWorldWithThisId(this ushort id)
        {
            return Worlds.Get(id);
        }
    }
}