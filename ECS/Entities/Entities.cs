using System;

namespace DesertImage.ECS
{
    internal static unsafe class Entities
    {
        internal static Entity GetNew(in World world)
        {
            var state = world.State;
            var pool = state->EntitiesPool;
            var id = pool.Count > 0 ? pool.Dequeue() : ++state->EntityIdCounter;

            state->AliveEntities.Set((int)id, id);
            Groups.OnEntityCreated(id, state);

            return new Entity(id, world.GetPtr());
        }

        internal static void DestroyEntity(in Entity entity, WorldState* state)
        {
#if DEBUG_MODE
            ThrowIfNotAlive(entity);
#endif
            var entityId = entity.Id;

            Groups.OnEntityDestroyed(entityId, state);
            Components.OnEntityDestroyed(entity, state);

            state->EntitiesPool.Enqueue(entityId);
            state->AliveEntities.Remove((int)entityId);
        }

        internal static Entity GetEntity(uint id, in World world) => new Entity(id, world.GetPtr());

#if DEBUG_MODE
        internal static void ThrowIfNotAlive(in Entity entity)
        {
            if (!entity.IsAlive()) throw new Exception($"Entity {entity} is not alive");
        }
#endif
    }
}