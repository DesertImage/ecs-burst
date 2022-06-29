using System;
using System.Collections.Generic;
using DesertImage.Events;
using DesertImage.Extensions;
using External;
using External.Extensions;
using Group;

namespace DesertImage.ECS
{
    public class SystemsManager :
        ITick,
        IDisposable,
        IListen<EntityAddedEvent>,
        IListen<EntityRemovedEvent>,
        IListen<EntityUpdatedEvent>,
        IListen<EntityPreUpdatedEvent>
    {
        private readonly IWorld _world;

        private readonly CustomDictionary<int, (ISystem, EntitiesGroup)> _systems =
            new CustomDictionary<int, (ISystem, EntitiesGroup)>(10, 3, -1);

        private readonly CustomDictionary<int, (ISystem, EntitiesGroup)> _executeSystems =
            new CustomDictionary<int, (ISystem, EntitiesGroup)>(10, 3, -1);

        private readonly CustomDictionary<int, (EntitiesGroup, List<IReactEntityAddedSystem>)> _entityAddedGroups =
            new CustomDictionary<int, (EntitiesGroup, List<IReactEntityAddedSystem>)>(10, 3, -1);

        private readonly CustomDictionary<int, (EntitiesGroup, List<IReactEntityRemovedSystem>)>
            _entityRemovedGroups =
                new CustomDictionary<int, (EntitiesGroup, List<IReactEntityRemovedSystem>)>(10, 3, -1);

        private readonly CustomDictionary<int, (EntitiesGroup, List<IReactivePreUpdateSystem>)>
            _reactivePreUpdateSystems =
                new CustomDictionary<int, (EntitiesGroup, List<IReactivePreUpdateSystem>)>(10, 3, -1);

        private readonly CustomDictionary<int, (EntitiesGroup, List<IReactiveUpdateSystem>)> _reactiveUpdateSystems =
            new CustomDictionary<int, (EntitiesGroup, List<IReactiveUpdateSystem>)>(10, 3, -1);

        private readonly List<IEndSystem> _endSystems = new List<IEndSystem>();

        public SystemsManager(IWorld world)
        {
            _world = world;
        }

        #region ADD

        public void AddSystem<T>() where T : class, ISystem, new()
        {
            AddSystem(new T());
        }

        private void AddSystem<T>(T system) where T : class, ISystem
        {
            var hashCode = HashCodeTypeTool.GetCachedHashCode<T>();

            if (system is IWorldInit worldInit)
            {
                worldInit.Init(_world);
            }

            system.Activate();

            if (system is IExecuteSystem executeSystem)
            {
                if (_executeSystems.TryGetValue(hashCode, out _))
                {
#if DEBUG
                    UnityEngine.Debug.LogWarning("[SystemsManager] system already added");
#endif
                    return;
                }

                var group = _world.GetGroup(executeSystem.Matcher);

                _systems.AddIfNotContains(hashCode, (executeSystem, group));

                _executeSystems.Add(hashCode, (executeSystem, group));
            }

            if (system is InitSystem initSystem)
            {
                initSystem.Initialize();
            }

            if (system is ITickSystem tickSystem)
            {
                _systems.AddIfNotContains(hashCode, (tickSystem, null));

                _executeSystems.AddIfNotContains(hashCode, (tickSystem, null));
            }

            if (system is IEndSystem endSystem)
            {
                _endSystems.Add(endSystem);
            }

            if (system is IReactivePreUpdateSystem reactivePreUpdateSystem)
            {
                var group = _world.GetGroup(reactivePreUpdateSystem.Matcher);

                group.ListenEvent<EntityPreUpdatedEvent>(this);

                _systems.AddIfNotContains(hashCode, (system, group));

                if (_reactivePreUpdateSystems.TryGetValue(group.Id, out var data))
                {
                    data.Item2.Add(reactivePreUpdateSystem);
                }
                else
                {
                    _reactivePreUpdateSystems.Add
                    (
                        group.Id,
                        (group, new List<IReactivePreUpdateSystem> { reactivePreUpdateSystem })
                    );
                }

                var componentId = reactivePreUpdateSystem.GetTargetComponentId();

                for (var i = 0; i < group.Entities.Count; i++)
                {
                    var entity = group.Entities[i];

                    var component = entity.Get(componentId);

                    reactivePreUpdateSystem.Execute(entity, component, component);
                }
            }

            if (system is IReactiveUpdateSystem reactiveSystem)
            {
                var group = _world.GetGroup(reactiveSystem.Matcher);

                group.ListenEvent<EntityUpdatedEvent>(this);

                _systems.AddIfNotContains(hashCode, (system, group));

                if (_reactiveUpdateSystems.TryGetValue(group.Id, out var data))
                {
                    data.Item2.Add(reactiveSystem);
                }
                else
                {
                    _reactiveUpdateSystems.Add
                    (
                        group.Id,
                        (group, new List<IReactiveUpdateSystem> { reactiveSystem })
                    );
                }

                for (var i = 0; i < group.Entities.Count; i++)
                {
                    reactiveSystem.Execute(group.Entities[i]);
                }
            }

            if (system is IReactEntityAddedSystem reactEntityAddedSystem)
            {
                var group = _world.GetGroup(reactEntityAddedSystem.Matcher);

                group.ListenEvent<EntityAddedEvent>(this);

                _systems.AddIfNotContains(hashCode, (system, group));

                if (_entityAddedGroups.TryGetValue(group.Id, out var data))
                {
                    data.Item2.Add(reactEntityAddedSystem);
                }
                else
                {
                    _entityAddedGroups.Add
                    (
                        group.Id,
                        (group, new List<IReactEntityAddedSystem> { reactEntityAddedSystem })
                    );
                }

                for (var i = 0; i < group.Entities.Count; i++)
                {
                    reactEntityAddedSystem.Execute(group.Entities[i]);
                }
            }

            if (system is IReactEntityRemovedSystem reactEntityRemovedSystem)
            {
                var group = _world.GetGroup(reactEntityRemovedSystem.Matcher);

                group.ListenEvent<EntityRemovedEvent>(this);

                _systems.AddIfNotContains(hashCode, (system, group));

                if (_entityRemovedGroups.TryGetValue(group.Id, out var data))
                {
                    data.Item2.Add(reactEntityRemovedSystem);
                }
                else
                {
                    _entityRemovedGroups.Add
                    (
                        group.Id,
                        (group, new List<IReactEntityRemovedSystem> { reactEntityRemovedSystem })
                    );
                }
            }
        }

        #endregion ADD

        #region REMOVE

        public void RemoveSystem<T>() where T : ISystem
        {
            var hashCode = HashCodeTypeTool.GetCachedHashCode<T>();

            if (!_systems.TryGetValue(hashCode, out var systemData)) return;

            _systems.Remove(hashCode);

            var (system, group) = systemData;

            if (system is IReactivePreUpdateSystem reactivePreUpdateSystem)
            {
                group.UnlistenEvent<EntityPreUpdatedEvent>(this);

                if (_reactivePreUpdateSystems.TryGetValue(group.Id, out var data))
                {
                    data.Item2?.Remove(reactivePreUpdateSystem);
                }
            }

            if (system is IReactiveUpdateSystem reactiveUpdateSystem)
            {
                group.UnlistenEvent<EntityUpdatedEvent>(this);

                if (_reactiveUpdateSystems.TryGetValue(group.Id, out var data))
                {
                    data.Item2?.Remove(reactiveUpdateSystem);
                }
            }

            if (system is IReactEntityAddedSystem reactEntityAddedSystem)
            {
                group.UnlistenEvent<EntityAddedEvent>(this);

                if (_entityAddedGroups.TryGetValue(group.Id, out var data))
                {
                    data.Item2?.Remove(reactEntityAddedSystem);
                }
            }

            if (system is IReactEntityRemovedSystem reactEntityRemovedSystem)
            {
                group.UnlistenEvent<EntityRemovedEvent>(this);

                if (_entityRemovedGroups.TryGetValue(group.Id, out var data))
                {
                    data.Item2?.Remove(reactEntityRemovedSystem);
                }
            }

            if (system is IExecuteSystem)
            {
                _executeSystems.Remove(hashCode);
            }

            system.Deactivate();
        }

        #endregion REMOVE

        public void Tick()
        {
            for (var i = 0; i < _executeSystems.Count; i++)
            {
                var (_, systemData) = _executeSystems[i];

                var (system, group) = systemData;

                switch (system)
                {
                    case ITickSystem tickSystem:
                        tickSystem.Tick();
                        break;

                    case IExecuteSystem executeSystem:
                    {
                        for (var j = 0; j < group.Entities.Count; j++)
                        {
                            executeSystem.Execute(group.Entities[j]);
                        }

                        break;
                    }
                }
            }
        }

        #region CALLBACKS

        public void Dispose()
        {
            foreach (var endSystem in _endSystems)
            {
                endSystem.ExecuteEnd();
            }

            _endSystems.Clear();
        }

        public void HandleCallback(EntityAddedEvent arguments)
        {
            var group = arguments.Group;
            var entity = arguments.Value;

            if (!_entityAddedGroups.TryGetValue(@group.Id, out var systemData)) return;

            var (_, systems) = systemData;

            foreach (var system in systems)
            {
                system.Execute(entity);

                //if entity no longer in group then quit
                if (!group.Contains(entity)) break;
            }
        }

        public void HandleCallback(EntityRemovedEvent arguments)
        {
            var group = arguments.Group;
            var entity = arguments.Value;

            if (!_entityRemovedGroups.TryGetValue(@group.Id, out var systemData)) return;

            var (_, systems) = systemData;

            foreach (var system in systems)
            {
                system.Execute(entity);

                //if entity no longer in group then quit
                if (!group.Contains(entity)) break;
            }
        }

        public void HandleCallback(EntityPreUpdatedEvent arguments)
        {
            var group = arguments.Group;
            var entity = arguments.Value;
            var currentValue = arguments.Previous;
            var newValue = arguments.Future;

            if (!_reactivePreUpdateSystems.TryGetValue(group.Id, out var systemData)) return;

            var (_, systems) = systemData;

            foreach (var system in systems)
            {
                if (currentValue.Id != system.GetTargetComponentId()) continue;

                system.Execute(entity, currentValue, newValue);
            }
        }

        public void HandleCallback(EntityUpdatedEvent arguments)
        {
            var group = arguments.Group;
            var entity = arguments.Value;
            var component = arguments.Component;

            if (!_reactiveUpdateSystems.TryGetValue(group.Id, out var systemData)) return;

            var (_, systems) = systemData;

            foreach (var system in systems)
            {
                if (!system.IsTriggerComponent(component)) continue;

                system.Execute(entity);
            }
        }

        #endregion CALLBACKS
    }
}