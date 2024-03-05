using DesertImage.Collections;
using Unity.Collections;

namespace DesertImage.ECS
{
    internal static unsafe class Groups
    {
        internal static ref EntitiesGroup GetNewGroup(World* world)
        {
            var state = world->State;
            var id = ++state->GroupIdCounter;
            state->Groups.Set(id, new EntitiesGroup(id, world, 20, 256));
            return ref state->Groups.Get(id);
        }

        internal static ref EntitiesGroup GetGroup(ushort id, WorldState* state) => ref state->Groups.Get(id);

        internal static void OnEntityCreated(uint entityId, WorldState* state)
        {
            if (state->EntityToGroups.Contains(entityId)) return;
            state->EntityToGroups.Set(entityId, new UnsafeList<ushort>(20, Allocator.Persistent));
        }

        internal static void OnEntityDestroyed(uint entityId, WorldState* state)
        {
            var entityGroups = state->EntityToGroups;

            if (!entityGroups.TryGetValue(entityId, out var groupsList)) return;

            for (var i = groupsList.Count - 1; i >= 0; i--)
            {
                ref var group = ref GetGroup(groupsList[i], state);
                group.Remove(entityId);
            }
        }

        internal static void OnEntityComponentAdded(in Entity entity, WorldState* state, uint componentId)
        {
            var entityId = entity.Id;

            for (var i = state->EntityToGroups[entityId].Count - 1; i >= 0; i--)
            {
                var groupId = state->EntityToGroups[entityId][i];
                var group = state->Groups[groupId];

                if (!group._components->Contains(componentId)) continue;
                if (group.IsValid(entityId)) continue;

                EntityRemove(entityId, groupId, state);
            }

            var componentGroups = GetComponentGroups(componentId, state);
            for (var i = componentGroups.Count - 1; i >= 0; i--)
            {
                var groupId = componentGroups[i];
                var group = state->Groups[groupId];

                var isValid = group.IsValid(entityId);
                var isContains = group.Contains(entityId);

                if (isContains)
                {
                    if (!isValid)
                    {
                        EntityRemove(entityId, groupId, state);
                    }

                    continue;
                }

                if (!isValid) continue;

                EntityAdd(entityId, groupId, state);
            }
        }

        internal static void OnEntityComponentRemoved(in Entity entity, WorldState* state, uint componentId)
        {
            var entityId = entity.Id;

            for (var i = state->EntityToGroups[entityId].Count - 1; i >= 0; i--)
            {
                var groupId = state->EntityToGroups[entityId][i];
                var group = state->Groups[groupId];

                if (!group._components->Contains(componentId)) continue;
                if (group.IsValid(entityId)) continue;

                EntityRemove(entityId, groupId, state);
            }

            var componentGroups = GetComponentGroups(componentId, state);
            for (var i = componentGroups.Count - 1; i >= 0; i--)
            {
                var groupId = componentGroups[i];
                var group = state->Groups[groupId];

                if (group.Contains(entityId)) continue;
                if (!group.IsValid(entityId)) continue;

                EntityAdd(entityId, groupId, state);
            }
        }

        internal static ref UnsafeList<ushort> GetComponentGroups(uint componentId, WorldState* state)
        {
            var componentGroups = state->ComponentToGroups;

            if (componentGroups.Contains(componentId)) return ref state->ComponentToGroups.Get(componentId);

            var groupsList = new UnsafeList<ushort>(20, Allocator.Persistent);

            for (var i = state->Groups.Count - 1; i >= 0; i--)
            {
                ref var group = ref state->Groups._dense[i];

                if (!group._components->Contains(componentId)) continue;

                groupsList.Add(group.Id);
            }

            state->ComponentToGroups.Set(componentId, groupsList);

            return ref state->ComponentToGroups.Get(componentId);
        }

        private static void EntityAdd(uint entityId, ushort groupId, WorldState* state)
        {
            state->EntityToGroups.Get(entityId).Add(groupId);
            state->Groups.Get(groupId).Add(entityId);
        }

        private static void EntityRemove(uint entityId, ushort groupId, WorldState* state)
        {
            state->EntityToGroups.Get(entityId).Remove(groupId);
            state->Groups.Get(groupId).Remove(entityId);
        }

        private static bool IsGroupContainsEntity(ushort groupId, uint entityId, WorldState* state)
        {
            return state->Groups.Get(groupId).Contains(entityId);
        }

        internal static void AddComponentGroup(uint componentId, ushort groupId, WorldState* state)
        {
            ref var componentGroups = ref GetComponentGroups(componentId, state);

#if DEBUG_MODE
            if (componentGroups.Contains(groupId))
            {
                throw new Exception($"Component groups ({componentId} already contains group: {groupId})");
            }
#endif

            componentGroups.Add(groupId);
        }
    }
}