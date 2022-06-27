using System.Collections.Generic;
using DesertImage.ECS;
using DesertImage.Pools;

namespace Group
{
    public class GroupsManager : IWorldInit
    {
        private readonly Dictionary<ushort, List<EntityGroup>> _entityGroups =
            new Dictionary<ushort, List<EntityGroup>>();

        private readonly Dictionary<IMatcher, EntityGroup> _matcherGroups =
            new Dictionary<IMatcher, EntityGroup>(new MatchersComparer());

        private readonly Dictionary<ushort, IMatcher> _groupMatchers = new Dictionary<ushort, IMatcher>();

        private readonly Dictionary<ushort, List<EntityGroup>> _componentGroups =
            new Dictionary<ushort, List<EntityGroup>>();

        //to avoid duplicated Update event during OnEntityUpdated callback
        private readonly HashSet<EntityGroup> _updatedGroups = new HashSet<EntityGroup>();
        private readonly HashSet<EntityGroup> _preUpdatedGroups = new HashSet<EntityGroup>();

        private readonly Pool<EntityGroup> _groupsPool = new EntityGroupsPool();

        public void Init(IWorld world)
        {
            world.OnEntityAdded += WorldOnEntityAdded;
            world.OnEntityRemoved += WorldOnEntityRemoved;

            world.OnEntityComponentAdded += WorldOnEntityComponentAddedOrRemoved;
            world.OnEntityComponentRemoved += WorldOnEntityComponentAddedOrRemoved;
            world.OnEntityComponentPreUpdated += WorldOnEntityComponentPreUpdated;
            world.OnEntityComponentUpdated += WorldOnEntityComponentUpdated;
        }

        public EntityGroup GetGroup(IMatcher matcher)
        {
            return _matcherGroups.TryGetValue(matcher, out var group) ? group : GetNewGroup(matcher);
        }

        public EntityGroup GetGroup(ushort componentId)
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

        public EntityGroup GetGroup(ushort[] componentIds)
        {
            foreach (var pair in _matcherGroups)
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

        private EntityGroup GetNewGroup()
        {
            return _groupsPool.GetInstance();
        }

        private EntityGroup GetNewGroup(IMatcher matcher)
        {
            var newGroup = GetNewGroup();

            _groupMatchers.Add(newGroup.Id, matcher);
            _matcherGroups.Add(matcher, newGroup);

            foreach (var componentId in matcher.ComponentIds)
            {
                if (_componentGroups.TryGetValue(componentId, out var groups))
                {
                    groups.Add(newGroup);
                }
                else
                {
                    _componentGroups.Add(componentId, new List<EntityGroup> { newGroup });
                }
            }

            return newGroup;
        }

        private void AddToGroup(EntityGroup group, IEntity entity)
        {
            group.Add(entity);

            if (_entityGroups.TryGetValue((ushort)entity.Id, out var groupsList))
            {
                groupsList.Add(group);
            }
            else
            {
                _entityGroups.Add((ushort)entity.Id, new List<EntityGroup> { group });
            }
        }

        private void RemoveFromGroup(EntityGroup group, IEntity entity)
        {
            group.Remove(entity);

            if (_entityGroups.TryGetValue((ushort)entity.Id, out var groupsList))
            {
                groupsList.Remove(group);
            }
        }

        #region CALLBACKS

        private void WorldOnEntityAdded(IEntity entity)
        {
            foreach (var pair in _matcherGroups)
            {
                var matcher = pair.Key;
                var group = pair.Value;

                if (!matcher.IsMatch(entity)) continue;

                AddToGroup(group, entity);
            }
        }

        private void WorldOnEntityRemoved(IEntity entity)
        {
            if (!_entityGroups.TryGetValue((ushort)entity.Id, out var groups)) return;

            for (var i = groups.Count - 1; i >= 0; i--)
            {
                groups[i].Remove(entity);
            }

            groups.Clear();

            return;
        }

        private void WorldOnEntityComponentAddedOrRemoved(IEntity entity, IComponent component)
        {
            if (_componentGroups.TryGetValue(component.Id, out var groups))
            {
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
        }

        private void WorldOnEntityComponentPreUpdated(IEntity entity, IComponent component, IComponent newValues)
        {
            if (!_entityGroups.TryGetValue((ushort)entity.Id, out var groups)) return;

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
            if (!_entityGroups.TryGetValue((ushort)entity.Id, out var groups)) return;

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