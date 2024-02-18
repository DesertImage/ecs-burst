using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public struct ComponentTools
    {
        private static readonly Unity.Burst.SharedStatic<uint> IDCounter = Unity.Burst.SharedStatic<uint>.GetOrCreate<ComponentTools>();

        public static uint GetComponentId<T>() where T : struct
        {
            var id = ComponentTypes<T>.TypeId.Data;

            if (id > 0) return id;

            id = ++IDCounter.Data;

            ComponentTypes<T>.TypeId.Data = id;
            // ComponentTypes<T>.MemorySize = UnsafeUtility.SizeOf<T>();

            return id;
        }
        
        // public static int GetSize<T>() where T : struct
        // {
        //     var id = ComponentTypes<T>.TypeId;
        //
        //     if (id >= 0) return ComponentTypes<T>.MemorySize;
        //
        //     id = ++_typesIdCounter;
        //
        //     ComponentTypes<T>.TypeId = id;
        //     // ComponentTypes<T>.MemorySize = UnsafeUtility.SizeOf<T>();
        //
        //     return id;
        // }
    }
}