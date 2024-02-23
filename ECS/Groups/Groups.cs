using DesertImage.Collections;
using Unity.Collections;
using UnityEngine;

namespace DesertImage.ECS
{
    internal static unsafe class Groups
    {
        private static ref EntitiesGroup GetNewGroup(WorldState* state)
        {
            var id = ++state->GroupIdCounter;
            state->Groups.Set(id, new EntitiesGroup(id));
            return ref state->Groups.Get(id);
        }

        internal static ushort GetSystemMatcherId(uint systemId, in World world)
        {
            var state = world.State;
            return (ushort)(state->SystemToMatcher.TryGetValue(systemId, out var matcherId) ? matcherId : 0);
        }

        internal static EntitiesGroup GetSystemGroup(uint systemId, in World world)
        {
            var state = world.State;

            if (state->SystemToMatcher.TryGetValue(systemId, out var matcherId))
            {
                var groupId = state->MatcherToGroup.Get(matcherId);
                return state->Groups.Get(groupId);
            }

            return default;
        }

        internal static void RegisterSystemMatcher(uint systemId, Matcher matcher, in World world)
        {
            var state = world.State;

            var matcherId = matcher.Id;

            state->SystemToMatcher.Set(systemId, matcherId);
            state->Matchers.Set(matcherId, matcher);

            if (!state->MatcherToGroup.Contains(matcher.Id))
            {
                GetGroup(matcher, world);
            }
        }

        internal static ref EntitiesGroup GetGroup(in Matcher matcher, in World world)
        {
            var state = world.State;
            var matcherId = matcher.Id;

            if (state->MatcherToGroup.TryGetValue(matcherId, out var groupId)) return ref GetGroup(groupId, state);

            return ref GetNewGroup(matcher, world);
        }

        private static ref EntitiesGroup GetNewGroup(in Matcher matcher, in World world)
        {
            var state = world.State;
            var matcherId = matcher.Id;

            ref var newGroup = ref GetNewGroup(state);
            var newGroupId = newGroup.Id;

            FillGroup(newGroupId, matcher, world);

            state->Matchers.Set(matcherId, matcher);
            state->MatcherToGroup.Set(matcherId, newGroupId);
            state->GroupToMatcher.Set(newGroupId, matcherId);

            foreach (var componentId in matcher.Components)
            {
                if (state->ComponentToGroups.Contains(componentId))
                {
                    state->ComponentToGroups.Get(componentId).Add(newGroupId);
                }
                else
                {
                    var list = new UnsafeList<ushort>(20, Allocator.Persistent);
                    list.Add(newGroupId);

                    state->ComponentToGroups.Set(componentId, list);
                }
            }

            return ref state->Groups.Get(newGroupId);
        }

        internal static ref EntitiesGroup GetGroup(ushort id, WorldState* state) => ref state->Groups.Get(id);

        internal static void OnEntityCreated(uint entityId, WorldState* state)
        {
            if (state->EntityToGroups.TryGetValue(entityId, out _)) return;
            state->EntityToGroups.Set(entityId, new UnsafeList<ushort>(20, Allocator.Persistent));
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

        internal static void OnEntityComponentAdded(in Entity entity, WorldState* state, uint componentId)
        {
            var entityId = entity.Id;

            for (var i = 0; i < state->EntityToGroups[entityId].Count; i++)
            {
                var groupId = state->EntityToGroups[entityId][i];
                var matcherId = state->GroupToMatcher[groupId];
                var matcher = state->Matchers.Get(matcherId);

                if(!matcher.Components.Contains(componentId)) continue;
                if (matcher.Check(entity)) continue;

                i--;
                EntityRemove(entityId, groupId, state);
            }

            var componentGroups = GetComponentGroups(componentId, state);
            for (var i = 0; i < componentGroups.Count; i++)
            {
                var groupId = componentGroups[i];
                
                if(IsGroupContainsEntity(groupId, entityId, state)) continue;
                
                var matcherId = state->GroupToMatcher[groupId];
                var matcher = state->Matchers.Get(matcherId);

                if (!matcher.Check(entity)) continue;

                EntityAdd(entityId, groupId, state);
            }
        }

        internal static void OnEntityComponentRemoved(in Entity entity, WorldState* state, uint componentId)
        {
            var entityId = entity.Id;

            for (var i = 0; i < state->EntityToGroups[entityId].Count; i++)
            {
                var groupId = state->EntityToGroups[entityId][i];
                var matcherId = state->GroupToMatcher[groupId];
                var matcher = state->Matchers.Get(matcherId);

                if(!matcher.Components.Contains(componentId)) continue;
                if (matcher.Check(entity)) continue;

                i--;
                EntityRemove(entityId, groupId, state);
            }

            var componentGroups = GetComponentGroups(componentId, state);
            for (var i = 0; i < componentGroups.Count; i++)
            {
                var groupId = componentGroups[i];
                
                if(IsGroupContainsEntity(groupId, entityId, state)) continue;

                var matcherId = state->GroupToMatcher[groupId];
                var matcher = state->Matchers.Get(matcherId);

                if (!matcher.Check(entity)) continue;

                EntityAdd(entityId, groupId, state);
            }
        }

        private static ref UnsafeList<ushort> GetComponentGroups(uint componentId, WorldState* state)
        {
            var componentGroups = state->ComponentToGroups;

            if (componentGroups.Contains(componentId)) return ref state->ComponentToGroups.Get(componentId);

            var groupsList = new UnsafeList<ushort>(20, Allocator.Persistent);

            foreach (var matcher in state->Matchers)
            {
                if (!matcher.Components.Contains(componentId)) continue;

                var groupId = state->MatcherToGroup[matcher.Id];
                groupsList.Add(groupId);
            }

            state->ComponentToGroups.Set(componentId, groupsList);

            return ref state->ComponentToGroups.Get(componentId);
        }

        private static void FillGroup(ushort groupId, in Matcher matcher, in World world)
        {
            var state = world.State;

            foreach (var entityId in state->AliveEntities)
            {
                var entity = Entities.GetEntity(entityId, world);

                if (!matcher.Check(entity)) continue;

                EntityAdd(entityId, groupId, state);
            }
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
    }
}