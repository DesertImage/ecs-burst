using System;

namespace DesertImage.ECS
{
    internal static unsafe class Entities
    {
        internal static Entity GetNew(World world)
        {
            var state = world.State;
            var pool = state->EntitiesPool;
            var id = pool.Count > 0 ? pool.Dequeue() : ++state->EntityIdCounter;

            state->AliveEntities.Add((int)id, id);

            Groups.OnEntityCreated(id, state);

            return new Entity(id, world.Id);
        }

        internal static void DestroyEntity(ref Entity entity, WorldState* state)
        {
            ThrowIfNotAlive(entity);

            var entityId = entity.Id;

            entity.IsAliveFlag = 0;

            Groups.OnEntityDestroyed(entityId, state);
            Components.OnEntityDestroyed(entity, state);

            state->EntitiesPool.Enqueue(entityId);
            state->AliveEntities.Remove((int)entityId);
        }

        internal static Entity GetEntity(uint id, World world) => new Entity(id, world.Id);

        internal static void ThrowIfNotAlive(Entity entity)
        {
            if (!entity.IsAlive()) throw new Exception($"Entity {entity} is not alive");
        }
    }
}