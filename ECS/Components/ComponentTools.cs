namespace DesertImage.ECS
{
    public struct ComponentTools
    {
        private static readonly Unity.Burst.SharedStatic<uint> IDCounter =
            Unity.Burst.SharedStatic<uint>.GetOrCreate<ComponentTools>();

        public static uint GetComponentId<T>() where T : struct
        {
            var id = ComponentTypes<T>.TypeId.Data;

            if (id > 0) return id;

            id = ++IDCounter.Data;

            ComponentTypes<T>.TypeId.Data = id;

            return id;
        }

        public static uint GetComponentIdFast<T>() where T : struct => ComponentTypes<T>.TypeId.Data;
    }
}