namespace DesertImage.ECS
{
    public static unsafe class WorldExtensions
    {
        public static Entity GetNewEntity(this in World world) => Entities.GetNew(world.Ptr);
        public static Entity GetEntity(this in World world, uint id) => new(id, world.Ptr);

        public static void Add<T>(this in World world, ExecutionOrder order = ExecutionOrder.MultiThread)
            where T : unmanaged, ISystem
        {
            Systems.Add<T>(world, order);
        }

        public static void AddRemoveComponentSystem<T>(this in World world,
            ExecutionOrder order = ExecutionOrder.RemoveTags)
            where T : unmanaged
        {
            Systems.Add<RemoveComponentSystem<T>>(world, order);
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

        public static T ReadStatic<T>(this in World world) where T : unmanaged
        {
            return Components.ReadStatic<T>(world.State);
        }

        public static ref T GetStatic<T>(this in World world) where T : unmanaged
        {
            return ref Components.GetStatic<T>(world.State);
        }

        public static void ReplaceStatic<T>(this in World world, T instance) where T : unmanaged
        {
            Components.ReplaceStatic(world.State, instance);
        }

        public static void Tick(this in World world, float deltaTime)
        {
            Systems.Execute(Worlds.GetPtr(world.Id), deltaTime);
        }

        public static void PhysicsTick(this in World world, float deltaTime)
        {
            Systems.ExecutePhysics(Worlds.GetPtr(world.Id), deltaTime);
        }

        public static void GizmosTick(this in World world)
        {
#if UNITY_EDITOR
            Systems.ExecuteGizmos(Worlds.GetPtr(world.Id));
#endif
        }

        public static World GetWorldWithThisId(this ushort id) => Worlds.Get(id);

        public static EntitiesGroup GetNewGroup(this in World world) => Groups.GetNewGroup(world.Ptr);
    }
}