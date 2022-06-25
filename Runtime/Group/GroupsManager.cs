using System.Collections.Generic;
using DesertImage;
using DesertImage.ECS;

namespace Group
{
    public class GroupsManager : IWorldInit
    {
        private static IWorld _world;

        //entityId, entityGroupsList
        private readonly Dictionary<ushort, List<EntityGroup>> _entityGroups =
            new Dictionary<ushort, List<EntityGroup>>();

        //to avoid duplicated Update event during OnEntityUpdated callback
        private readonly HashSet<EntityGroup> _updatedGroups = new HashSet<EntityGroup>();
        private readonly HashSet<EntityGroup> _preUpdatedGroups = new HashSet<EntityGroup>();

        public void Init(IWorld world)
        {
            _world = world;

            world.OnEntityAdded += WorldOnEntityAdded;
            world.OnEntityRemoved += WorldOnEntityRemoved;

            world.OnEntityComponentAdded += WorldOnEntityComponentAddedOrRemoved;
            world.OnEntityComponentRemoved += WorldOnEntityComponentAddedOrRemoved;
            world.OnEntityComponentPreUpdated += WorldOnEntityComponentPreUpdated;
            world.OnEntityComponentUpdated += WorldOnEntityComponentUpdated;

            world.OnGroupAdded += WorldOnGroupAdded;
        }

        private void AddToGroup(EntityGroup group, IEntity entity)
        {
            group.Add(entity);

            if (_entityGroups.TryGetValue((ushort)entity.Id, out var groupsList))
            {
                if (group.Id == 58)
                {
                    #if DEBUG
                    UnityEngine.Debug.LogError("<b>[GroupsManager]</b> alsdkjlaksdj");
                    #endif
                    
                }
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
                if (group.Id == 58)
                {
#if DEBUG
                    UnityEngine.Debug.LogError("<b>[GroupsManager]</b> asdasdasdasd");
#endif
                }
                
                groupsList.Remove(group);
            }
        }

        #region CALLBACKS

        private void WorldOnEntityAdded(IEntity entity)
        {
            for (var i = 0; i < _world.Groups.Count; i++)
            {
                var (matcher, group) = _world.Groups[i];

                if (!matcher.IsMatch(entity)) continue;

                AddToGroup(group, entity);
            }
        }

        private void WorldOnEntityRemoved(IEntity entity)
        {
            if (_entityGroups.TryGetValue((ushort)entity.Id, out var groups))
            {
                for (var i = groups.Count - 1; i >= 0; i--)
                {
                    groups[i].Remove(entity);
                }

                groups.Clear();

                return;
            }

            for (var i = 0; i < _world.Groups.Count; i++)
            {
                var (_, group) = _world.Groups[i];

                if (!group.Contains(entity)) continue;

                RemoveFromGroup(group, entity);
            }
        }

        private void WorldOnEntityComponentAddedOrRemoved(IEntity entity, IComponent component)
        {
            if (_world.ComponentGroups.TryGetValue(component.Id, out var groupInfos))
            {
                foreach (var groupInfo in groupInfos)
                {
                    var (matcher, group) = groupInfo;

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

                return;
            }

            for (var i = 0; i < _world.Groups.Count; i++)
            {
                if (component == null) continue;

                var (matcher, group) = _world.Groups[i];

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
            if (_entityGroups.TryGetValue((ushort)entity.Id, out var groups))
            {
                for (var i = 0; i < groups.Count; i++)
                {
                    var group = groups[i];

                    group.PreUpdate(entity, component, newValues);
                }

                return;
            }

            for (var i = 0; i < _world.Groups.Count; i++)
            {
                var (matcher, group) = _world.Groups[i];

                var isContainsEntity = group.Contains(entity);

                if (!isContainsEntity) continue;

                if (!matcher.IsContainsComponent(component.Id)) continue;

                if (_preUpdatedGroups.Contains(group)) continue;
                
                group.PreUpdate(entity, component, newValues);

                _preUpdatedGroups.Add(group);
            }

            _preUpdatedGroups.Clear();
        }

        private void WorldOnEntityComponentUpdated(IEntity entity, IComponent component)
        {
            if (_entityGroups.TryGetValue((ushort)entity.Id, out var groups))
            {
                for (var i = 0; i < groups.Count; i++)
                {
                    var group = groups[i];
            
                    if (_updatedGroups.Contains(group)) continue;
                    
                    group.Update(entity, component);

                    _updatedGroups.Add(group);
                }
                
                _updatedGroups.Clear();
            
                return;
            }

            for (var i = 0; i < _world.Groups.Count; i++)
            {
                var (matcher, group) = _world.Groups[i];

                var isContainsEntity = group.Contains(entity);

                if (!isContainsEntity) continue;

                if (!matcher.IsContainsComponent(component.Id)) continue;

                group.Update(entity, component);
            }
        }

        private void WorldOnGroupAdded(IMatcher matcher, EntityGroup group)
        {
            foreach (var entity in _world.Entities)
            {
                if (!matcher.IsMatch(entity)) continue;

                AddToGroup(group, entity);
            }
        }

        #endregion
    }
}