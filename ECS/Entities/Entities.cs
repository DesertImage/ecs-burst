using System;

namespace DesertImage.ECS
{
    internal static unsafe class Entities
    {
        internal static Entity GetNew(World* world)
        {
            var state = world->State;
            var pool = state->EntitiesPool;
            var id = pool.Count > 0 ? pool.Dequeue() : ++state->EntityIdCounter;

            state->AliveEntities.Add(id, id);
#if ECS_AUTODESTROY_ENTITY
            state->EntityComponentsCount.Add(id, 0);
#endif
            Groups.OnEntityCreated(id, state);

            return new Entity(id, world);
        }

        internal static void DestroyEntity(this in Entity entity)
        {
#if DEBUG_MODE
            ThrowIfNotAlive(entity);
#endif
            var entityId = entity.Id;
            var state = entity.World->State;

            Groups.OnEntityDestroyed(entityId, state);
            Components.OnEntityDestroyed(entity, state);

            state->EntitiesPool.Enqueue(entityId);
            state->AliveEntities.Remove(entityId);
#if ECS_AUTODESTROY_ENTITY
            state->EntityComponentsCount.Remove(entityId);
#endif
        }
        
        internal static void DestroyEntity(uint entityId, WorldState* state)
        {
            Groups.OnEntityDestroyed(entityId, state);
            Components.OnEntityDestroyed(entityId, state);

            state->EntitiesPool.Enqueue(entityId);
            state->AliveEntities.Remove(entityId);
        }

        internal static Entity GetEntity(uint id, World* world) => new Entity(id, world);

#if DEBUG_MODE
        internal static void ThrowIfNotAlive(in Entity entity)
        {
            if (!entity.IsAlive()) throw new Exception($"Entity {entity.Id} is not alive");
        }
#endif
    }
}