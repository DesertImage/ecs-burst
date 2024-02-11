using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public static class ComponentTools
    {
        private static int _typesIdCounter;

        //TODO: refactor
        public static int GetComponentId<T>() where T : struct
        {
            var id = ComponentTypes<T>.TypeId;

            if (id >= 0) return id;

            id = ++_typesIdCounter;

            ComponentTypes<T>.TypeId = id;
            ComponentTypes<T>.MemorySize = UnsafeUtility.SizeOf<T>();

            return id;
        }
        
        public static int GetSize<T>() where T : struct
        {
            var id = ComponentTypes<T>.TypeId;

            if (id >= 0) return ComponentTypes<T>.MemorySize;

            id = ++_typesIdCounter;

            ComponentTypes<T>.TypeId = id;
            ComponentTypes<T>.MemorySize = UnsafeUtility.SizeOf<T>();

            return id;
        }
    }
}