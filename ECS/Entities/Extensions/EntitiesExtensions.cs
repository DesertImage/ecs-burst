using DesertImage.Collections;

namespace DesertImage.ECS
{
    public static unsafe class EntitiesExtensions
    {
        public static void Destroy(this in Entity entity) => entity.DestroyEntity();

        public static bool IsAlive(this in Entity entity)
        {
            return entity.IsAliveFlag == 1 && entity.World->State->AliveEntities.Contains(entity.Id);
        }

        public static BufferArray<T> CreateBufferArray<T>(this in Entity entity, int length)
            where T : unmanaged
        {
            return Components.CreateBufferArray<T>
            (
                entity.Id,
                0,
                length,
                entity.World->State
            );
        }

        public static BufferList<T> CreateBufferList<T>(this in Entity entity, int capacity = 10)
            where T : unmanaged
        {
            return Components.CreateBufferList<T>
            (
                entity.Id,
                0,
                capacity,
                entity.World->State
            );
        }

        public static BufferList<T> CreateBufferList<T, TComponent>(this in Entity entity, int capacity = 10)
            where T : unmanaged where TComponent : struct
        {
            return Components.CreateBufferList<T>
            (
                entity.Id,
                ComponentTools.GetComponentId<TComponent>(),
                capacity,
                entity.World->State
            );
        }

        public static BufferStack<T> CreateBufferStack<T>(this in Entity entity, int capacity = 10)
            where T : unmanaged
        {
            return Components.CreateBufferStack<T>
            (
                entity.Id,
                0,
                capacity,
                entity.World->State
            );
        }

        public static BufferUintSparseSet<T> CreateBufferSparseSet<T>(this in Entity entity, int capacity = 10)
            where T : unmanaged
        {
            return Components.CreateBufferSparseList<T>
            (
                entity.Id,
                0,
                capacity,
                entity.World->State
            );
        }

        public static BufferUintSparseSet<T> CreateBufferSparseSet<T, TComponent>(this in Entity entity,
            int capacity = 10)
            where T : unmanaged where TComponent : struct
        {
            return Components.CreateBufferSparseList<T>
            (
                entity.Id,
                ComponentTools.GetComponentId<TComponent>(),
                capacity,
                entity.World->State
            );
        }
    }
}