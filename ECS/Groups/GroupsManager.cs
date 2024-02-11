using System;
using Unity.Collections;

namespace DesertImage.ECS
{
    public unsafe struct GroupsManager : IDisposable
    {
        private UnsafeDictionary<int, UnsafeList<int>> _entityGroups;
        private UnsafeDictionary<int, int> _matcherGroups;

        private UnsafeDictionary<int, Matcher> _groupMatchers;
        private UnsafeDictionary<int, UnsafeList<int>> _componentGroups;
        private UnsafeDictionary<int, UnsafeList<int>> _noneOfComponentGroups;

        private UnsafeList<EntitiesGroup> _groups;

        private readonly EntitiesManager* _entitiesManager;

        private static int _groupsIdCounter = -1;

        public GroupsManager(EntitiesManager* entitiesManager)
        {
            _entitiesManager = entitiesManager;

            _entityGroups = new UnsafeDictionary<int, UnsafeList<int>>(20, Allocator.Persistent);
            _matcherGroups = new UnsafeDictionary<int, int>(20, Allocator.Persistent);

            _groupMatchers = new UnsafeDictionary<int, Matcher>(20, Allocator.Persistent);
            _componentGroups = new UnsafeDictionary<int, UnsafeList<int>>(20, Allocator.Persistent);
            _noneOfComponentGroups = new UnsafeDictionary<int, UnsafeList<int>>(20, Allocator.Persistent);

            _groups = new UnsafeList<EntitiesGroup>(20, Allocator.Persistent);

            _groupsIdCounter = -1;
        }

        public EntitiesGroup GetGroup(Matcher matcher)
        {
            return _matcherGroups.TryGetValue(matcher.Id, out var group) ? _groups[group - 1] : GetNewGroup(matcher);
        }

        private EntitiesGroup GetNewGroup()
        {
            var newGroup = new EntitiesGroup(++_groupsIdCounter);
            _groups.Add(newGroup);
            return newGroup;
        }

        private EntitiesGroup GetNewGroup(Matcher matcher)
        {
            var newGroup = GetNewGroup();

            var newGroupId = newGroup.Id;

            _groupMatchers.Add(newGroupId, matcher);
            _matcherGroups.Add(matcher.Id, newGroupId);

            for (var i = 0; i < matcher.Components.Length; i++)
            {
                var componentId = matcher.Components[i];

                if (_componentGroups.TryGetValue(componentId, out var groups))
                {
                    groups.Add(newGroupId);
                }
                else
                {
                    _componentGroups.Add
                    (
                        componentId,
                        new UnsafeList<int>(20, Allocator.Persistent) { newGroupId }
                    );
                }
            }

            for (var i = 0; i < matcher.NoneOfComponents.Length; i++)
            {
                var componentId = matcher.NoneOfComponents[i];

                if (_noneOfComponentGroups.TryGetValue(componentId, out var groups))
                {
                    groups.Add(newGroupId);
                }
                else
                {
                    _noneOfComponentGroups.Add
                    (
                        componentId,
                        new UnsafeList<int>(20, Allocator.Persistent) { newGroupId }
                    );
                }
            }

            return newGroup;
        }

        private void AddToGroup(ref EntitiesGroup group, int entityId)
        {
            var groupId = group.Id;

            group.Add(entityId);

            ref var groups = ref _entityGroups;

            if (groups.TryGetValue(entityId, out var groupsList))
            {
                groupsList.Add(groupId);
            }
            else
            {
                groups.Add
                (
                    entityId,
                    new UnsafeList<int>(20, Allocator.Persistent) { groupId }
                );
            }
        }

        private void RemoveFromGroup(ref EntitiesGroup group, int entityId)
        {
            group.Remove(entityId);

            var entityGroup = _entityGroups[entityId];
            entityGroup.RemoveAt(entityGroup.IndexOf(group.Id));
        }

        public void OnEntityCreated(int entityId)
        {
            ref var groups = ref _entityGroups;
            groups.Add(entityId, new UnsafeList<int>(20, Allocator.Persistent, default));

            var entityGroup = _entityGroups[entityId];
        }

        public void OnEntityComponentAdded(int entityId, int componentId)
        {
            var groups = _entityGroups[entityId];

            for (var i = groups.Count - 1; i >= 0; i--)
            {
                var groupId = groups[i];

                var matcher = _groupMatchers[groupId];
                var components = _entitiesManager->GetComponents(entityId);

                if (matcher.Check(components)) continue;

                ref var group = ref _groups.GetByRef(groupId);
                RemoveFromGroup(ref group, entityId);
            }

            if (_componentGroups.TryGetValue(componentId, out var componentGroups))
            {
                ValidateEntityAdd(entityId, componentGroups);
            }

            if (_noneOfComponentGroups.TryGetValue(componentId, out componentGroups))
            {
                ValidateEntityRemove(entityId, componentGroups);
            }
        }

        private void ValidateEntityAdd(int entityId, UnsafeList<int> groups)
        {
            for (var i = groups.Count - 1; i >= 0; i--)
            {
                var groupId = groups[i];
                var group = _groups.GetByRef(groupId);

                if (group.Contains(entityId)) continue;

                var matcher = _groupMatchers[groupId];
                var components = _entitiesManager->GetComponents(entityId);

                if (!matcher.Check(components)) continue;

                AddToGroup(ref group, entityId);
            }
        }

        //TODO:refactor
        private void ValidateEntityRemove(int entityId, UnsafeList<int> groups)
        {
            for (var i = groups.Count - 1; i >= 0; i--)
            {
                var groupId = groups[i];
                ref var group = ref _groups.GetByRef(groupId);

                if (!group.Contains(entityId)) continue;

                var matcher = _groupMatchers[groupId];
                var components = _entitiesManager->GetComponents(entityId);

                if (matcher.Check(components)) continue;

                RemoveFromGroup(ref group, entityId);
            }
        }

        public void OnEntityComponentRemoved(int entityId, int componentId)
        {
            var groups = _entityGroups[entityId];

            var isAlive = _entitiesManager->IsAlive(entityId);

            if (!isAlive)
            {
                for (var i = groups.Count - 1; i >= 0; i--)
                {
                    var groupId = groups[i];
                    ref var group = ref _groups.GetByRef(groupId);

                    RemoveFromGroup(ref group, entityId);
                }
            }
            else
            {
                for (var i = groups.Count - 1; i >= 0; i--)
                {
                    var groupId = groups[i];

                    var matcher = _groupMatchers[groupId];
                    var components = _entitiesManager->GetComponents(entityId);

                    if (matcher.Check(components)) continue;

                    ref var group = ref _groups.GetByRef(groupId);
                    RemoveFromGroup(ref group, entityId);
                }

                if (!_noneOfComponentGroups.TryGetValue(componentId, out var componentGroups)) return;

                for (var i = componentGroups.Count - 1; i >= 0; i--)
                {
                    var groupId = componentGroups[i];
                    ref var group = ref _groups.GetByRef(groupId);

                    var matcher = _groupMatchers[groupId];
                    var components = _entitiesManager->GetComponents(entityId);

                    if (!matcher.Check(components)) continue;

                    AddToGroup(ref group, entityId);
                }
            }
        }

        public void Dispose()
        {
            foreach (var entityGroup in _entityGroups)
            {
                entityGroup.Value.Dispose();
            }

            foreach (var groupMatcher in _groupMatchers)
            {
                groupMatcher.Value.Dispose();
            }

            foreach (var componentGroup in _componentGroups)
            {
                componentGroup.Value.Dispose();
            }

            foreach (var componentGroup in _noneOfComponentGroups)
            {
                componentGroup.Value.Dispose();
            }

            foreach (var entitiesGroup in _groups)
            {
                entitiesGroup.Dispose();
            }

            _groupMatchers.Dispose();
            _entityGroups.Dispose();
            _matcherGroups.Dispose();
            _componentGroups.Dispose();
            _noneOfComponentGroups.Dispose();
            _groups.Dispose();
        }
    }
}