using DesertImage.Collections;
using Unity.Collections;

namespace DesertImage.ECS
{
    internal static unsafe class Groups
    {
        private static ref EntitiesGroup GetNewGroup(WorldState* state)
        {
            var id = ++state->GroupIdCounter;
            state->Groups.Add(id, new EntitiesGroup(id));
            return ref state->Groups.GetByRef(id);
        }

        internal static ref EntitiesGroup GetGroup(Matcher matcher, World world)
        {
            var state = world.State;

            var matcherId = matcher.Id;

            if (state->MatcherToGroup.TryGetValue(matcherId, out var groupId)) return ref GetGroup(groupId, state);

            ref var newGroup = ref GetNewGroup(state);
            var newGroupId = newGroup.Id;

            FillGroup(newGroupId, matcher, world);

            state->Matchers.Add(matcherId, matcher);
            state->MatcherToGroup.Add(matcherId, newGroupId);
            state->GroupToMatcher.Add(newGroupId, matcherId);

            foreach (var componentId in matcher.Components)
            {
                if (state->ComponentToGroups.Contains(componentId))
                {
                    state->ComponentToGroups.GetByRef(componentId).Add(newGroupId);
                }
                else
                {
                    state->ComponentToGroups.Add
                    (
                        componentId,
                        new UnsafeList<uint>(20, Allocator.Persistent) { newGroupId }
                    );
                }
            }

            return ref state->Groups.GetByRef(newGroupId);
        }

        internal static ref EntitiesGroup GetGroup(uint id, WorldState* state) => ref state->Groups.GetByRef(id);

        internal static void OnEntityCreated(uint entityId, WorldState* state)
        {
            if (state->EntityToGroups.TryGetValue(entityId, out _)) return;
            state->EntityToGroups.Add(entityId, new UnsafeList<uint>(20, Allocator.Persistent));
        }

        internal static void OnEntityDestroyed(uint entityId, WorldState* state)
        {
            var entityGroups = state->EntityToGroups;

            if (!entityGroups.TryGetValue(entityId, out var groupsList)) return;

            for (var i = 0; i < groupsList.Count; i++)
            {
                ref var group = ref GetGroup(groupsList[i], state);
                group.Remove(entityId);
            }
        }

        internal static void OnEntityComponentAdded(Entity entity, WorldState* state, uint componentId)
        {
            var entityId = entity.Id;

            var entityGroups = state->EntityToGroups[entityId];
            for (var i = 0; i < entityGroups.Count; i++)
            {
                var groupId = entityGroups[i];
                var matcherId = state->GroupToMatcher[groupId];
                var matcher = state->Matchers.GetByRef(matcherId);

                if (matcher.Check(entity)) continue;

                EntityRemove(entityId, groupId, state);
            }

            var componentGroups = GetComponentGroups(componentId, state);
            for (var i = 0; i < componentGroups.Count; i++)
            {
                var groupId = componentGroups[i];
                var matcherId = state->GroupToMatcher[groupId];
                var matcher = state->Matchers.GetByRef(matcherId);

                if (!matcher.Check(entity)) continue;

                EntityAdd(entityId, groupId, state);
            }
        }

        internal static void OnEntityComponentRemoved(Entity entity, WorldState* state, uint componentId)
        {
            var entityId = entity.Id;

            for (var i = 0; i < state->EntityToGroups[entityId].Count; i++)
            {
                var groupId = state->EntityToGroups[entityId][i];
                var matcherId = state->GroupToMatcher[groupId];
                var matcher = state->Matchers.GetByRef(matcherId);

                if (matcher.HasAnyOf(entity) || matcher.HasAll(entity)) continue;

                EntityRemove(entityId, groupId, state);
            }

            var componentGroups = GetComponentGroups(componentId, state);
            for (var i = 0; i < componentGroups.Count; i++)
            {
                var groupId = componentGroups[i];
                var matcherId = state->GroupToMatcher[groupId];
                var matcher = state->Matchers.GetByRef(matcherId);

                if (!matcher.HasNot(entity)) continue;

                EntityAdd(entityId, groupId, state);
            }
        }

        private static ref UnsafeList<uint> GetComponentGroups(uint componentId, WorldState* state)
        {
            var componentGroups = state->ComponentToGroups;

            if (componentGroups.Contains(componentId)) return ref state->ComponentToGroups.GetByRef(componentId);

            var groupsList = new UnsafeList<uint>(20, Allocator.Persistent);

            foreach (var pair in state->MatcherToGroup)
            {
                var matcherId = pair.Key;
                var groupId = pair.Value;
                var matcher = state->Matchers.GetByRef(matcherId);

                if (!matcher.Components.Contains(componentId)) continue;

                groupsList.Add(groupId);
            }

            state->ComponentToGroups.Add(componentId, groupsList);

            return ref state->ComponentToGroups.GetByRef(componentId);
        }

        private static void FillGroup(uint groupId, Matcher matcher, World world)
        {
            var state = world.State;
            foreach (var entityId in state->AliveEntities)
            {
                var entity = Entities.GetEntity(entityId, world);

                if (matcher.Check(entity)) continue;

                EntityAdd(entityId, groupId, state);
            }
        }

        private static void EntityAdd(uint entityId, uint groupId, WorldState* state)
        {
            if (state->EntityToGroups.Contains(entityId))
            {
                state->EntityToGroups.GetByRef(entityId).Add(groupId);
            }

            state->Groups.GetByRef(groupId).Add(entityId);
        }

        private static void EntityRemove(uint entityId, uint groupId, WorldState* state)
        {
            if (state->EntityToGroups.Contains(entityId))
            {
                state->EntityToGroups.GetByRef(entityId).Remove(groupId);
            }

            state->Groups.GetByRef(groupId).Remove(entityId);
        }
    }
}