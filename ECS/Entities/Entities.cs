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

            state->AliveEntities.Add((int)id, id);

            Groups.OnEntityCreated(id, state);

            return new Entity(id, world.Id);
        }

        internal static void DestroyEntity(in Entity entity, WorldState* state)
        {
            ThrowIfNotAlive(entity);

            var entityId = entity.Id;

            // state->AliveEntities.Get((int)entity.Id)
            // entity.IsAliveFlag = 0;

            Groups.OnEntityDestroyed(entityId, state);
            Components.OnEntityDestroyed(entity, state);

            state->EntitiesPool.Enqueue(entityId);
            state->AliveEntities.Remove((int)entityId);
        }

        internal static Entity GetEntity(uint id, in World world) => new Entity(id, world.Id);

        internal static void ThrowIfNotAlive(in Entity entity)
        {
            if (!entity.IsAlive()) throw new Exception($"Entity {entity} is not alive");
        }
    }
}