using System.Collections.Generic;
using DesertImage.Events;
using DesertImage.Pools;

namespace DesertImage.ECS
{
    public class GroupsManager : EventUnit
    {
        public Dictionary<ushort, List<EntitiesGroup>> EntityGroups { get; } =
            new Dictionary<ushort, List<EntitiesGroup>>();

        public Dictionary<IMatcher, EntitiesGroup> MatcherGroups { get; } =
            new Dictionary<IMatcher, EntitiesGroup>(new MatchersComparer());

        private readonly Dictionary<ushort, IMatcher> _groupMatchers = new Dictionary<ushort, IMatcher>();

        private readonly Dictionary<ushort, List<EntitiesGroup>> _componentGroups =
            new Dictionary<ushort, List<EntitiesGroup>>();

        //to avoid duplicated Update event during OnEntityUpdated callback
        private readonly HashSet<EntitiesGroup> _updatedGroups = new HashSet<EntitiesGroup>();
        private readonly HashSet<EntitiesGroup> _preUpdatedGroups = new HashSet<EntitiesGroup>();

        private readonly Pool<EntitiesGroup> _groupsPool = new Pool<EntitiesGroup>();

        public GroupsManager(IWorld world)
        {
            world.OnEntityAdded += WorldOnEntityAdded;
            world.OnEntityRemoved += WorldOnEntityRemoved;

            world.OnEntityComponentAdded += WorldOnEntityComponentAddedOrRemoved;
            world.OnEntityComponentRemoved += WorldOnEntityComponentAddedOrRemoved;
            world.OnEntityComponentPreUpdated += WorldOnEntityComponentPreUpdated;
            world.OnEntityComponentUpdated += WorldOnEntityComponentUpdated;
        }

        public EntitiesGroup GetGroup(IMatcher matcher)
        {
            return MatcherGroups.TryGetValue(matcher, out var group) ? group : GetNewGroup(matcher);
        }

        public EntitiesGroup GetGroup(ushort componentId)
        {
            if (_componentGroups.TryGetValue(componentId, out var groups))
            {
                foreach (var group in groups)
                {
                    var groupMatcher = _groupMatchers[group.Id];

                    if (groupMatcher.ComponentIds.Length > 1) continue;

                    return group;
                }
            }

            return GetNewGroup(Match.AllOf(componentId));
        }

        public EntitiesGroup GetGroup(ushort[] componentIds)
        {
            foreach (var pair in MatcherGroups)
            {
                var matcher = pair.Key;
                var group = pair.Value;

                if (matcher.ComponentIds.Length != componentIds.Length) break;

                var isMatch = true;

                foreach (var componentId in componentIds)
                {
                    if (matcher.IsContainsComponent(componentId)) continue;

                    isMatch = false;

                    break;
                }

                return isMatch ? group : GetNewGroup(Match.AllOf(componentIds));
            }

            return GetNewGroup(Match.AllOf(componentIds));
        }

        private EntitiesGroup GetNewGroup()
        {
            return _groupsPool.GetInstance();
        }

        private EntitiesGroup GetNewGroup(IMatcher matcher)
        {
            var newGroup = GetNewGroup();

            _groupMatchers.Add(newGroup.Id, matcher);
            MatcherGroups.Add(matcher, newGroup);

            foreach (var componentId in matcher.ComponentIds)
            {
                if (_componentGroups.TryGetValue(componentId, out var groups))
                {
                    groups.Add(newGroup);
                }
                else
                {
                    _componentGroups.Add(componentId, new List<EntitiesGroup> { newGroup });
                }
            }

            EventsManager.Send(new GroupAddedEvent
            {
                Matcher = matcher,
                Value = newGroup
            });

            return newGroup;
        }

        private void AddToGroup(EntitiesGroup group, IEntity entity)
        {
            group.Add(entity);

            if (EntityGroups.TryGetValue((ushort)entity.Id, out var groupsList))
            {
                groupsList.Add(group);
            }
            else
            {
                EntityGroups.Add((ushort)entity.Id, new List<EntitiesGroup> { group });
            }
        }

        private void RemoveFromGroup(EntitiesGroup group, IEntity entity)
        {
            group.Remove(entity);

            if (EntityGroups.TryGetValue((ushort)entity.Id, out var groupsList))
            {
                groupsList.Remove(group);
            }
        }

        #region CALLBACKS

        private void WorldOnEntityAdded(IEntity entity)
        {
            foreach (var pair in MatcherGroups)
            {
                var matcher = pair.Key;
                var group = pair.Value;

                if (!matcher.IsMatch(entity)) continue;

                AddToGroup(group, entity);
            }
        }

        private void WorldOnEntityRemoved(IEntity entity)
        {
            if (!EntityGroups.TryGetValue((ushort)entity.Id, out var groups)) return;

            for (var i = groups.Count - 1; i >= 0; i--)
            {
                groups[i].Remove(entity);
            }

            groups.Clear();

            return;
        }

        private void WorldOnEntityComponentAddedOrRemoved(IEntity entity, IComponent component)
        {
            if (!_componentGroups.TryGetValue(component.Id, out var groups)) return;

            foreach (var group in groups)
            {
                var matcher = _groupMatchers[group.Id];

                if (!matcher.IsContainsComponent(component.Id)) continue;

                var isContainsEntity = group.Contains(entity);

                if (!matcher.IsMatch(entity))
                {
                    if (isContainsEntity)
                    {
                        RemoveFromGroup(group, entity);
                    }

                    continue;
                }

                if (isContainsEntity) continue;

                AddToGroup(group, entity);
            }
        }

        private void WorldOnEntityComponentPreUpdated(IEntity entity, IComponent component, IComponent newValues)
        {
            if (!EntityGroups.TryGetValue((ushort)entity.Id, out var groups)) return;

            for (var i = 0; i < groups.Count; i++)
            {
                var group = groups[i];

                if (_preUpdatedGroups.Contains(group)) continue;

                group.PreUpdate(entity, component, newValues);

                _preUpdatedGroups.Add(group);
            }

            _preUpdatedGroups.Clear();
        }

        private void WorldOnEntityComponentUpdated(IEntity entity, IComponent component)
        {
            if (!EntityGroups.TryGetValue((ushort)entity.Id, out var groups)) return;

            for (var i = 0; i < groups.Count; i++)
            {
                var group = groups[i];

                if (_updatedGroups.Contains(group)) continue;

                group.Update(entity, component);

                _updatedGroups.Add(group);
            }

            _updatedGroups.Clear();
        }

        #endregion
    }
}