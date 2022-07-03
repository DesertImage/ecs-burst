using System;
using DesertImage.Events;

namespace DesertImage.ECS
{
    public interface IWorld
    {
        event Action<IEntity> OnEntityAdded;
        event Action<IEntity> OnEntityRemoved;

        event Action<IEntity, IComponent> OnEntityComponentAdded;
        event Action<IEntity, IComponent> OnEntityComponentRemoved;
        event Action<IEntity, IComponent, IComponent> OnEntityComponentPreUpdated;
        event Action<IEntity, IComponent> OnEntityComponentUpdated;
        event Action<IEntity> OnEntityDisposed;

        IEntity GetNewEntity();
        IEntity GetNewEntity(Action<IEntity> setup);

        EntitiesGroup GetGroup(IMatcher matcher);
        EntitiesGroup GetGroup(ushort componentId);
        EntitiesGroup GetGroup(ushort[] componentIds);
    }

    public class World : IWorld,
        IListen<ComponentAddedEvent>,
        IListen<ComponentRemovedEvent>,
        IListen<ComponentPreUpdatedEvent>,
        IListen<ComponentUpdatedEvent>,
        IListen<DisposedEvent>,
        IListen<GroupAddedEvent>
    {
        public event Action<IEntity> OnEntityAdded;
        public event Action<IEntity> OnEntityRemoved;

        public event Action<IEntity, IComponent> OnEntityComponentAdded;
        public event Action<IEntity, IComponent> OnEntityComponentRemoved;
        public event Action<IEntity, IComponent, IComponent> OnEntityComponentPreUpdated;
        public event Action<IEntity, IComponent> OnEntityComponentUpdated;
        public event Action<IEntity> OnEntityDisposed;

        private readonly EntitiesManager _entitiesManager;
        private readonly GroupsManager _groupsManager;

        public World(GroupsManager groupsManager = null, EntitiesManager entitiesManager = null)
        {
            _groupsManager = groupsManager ?? new GroupsManager(this);
            _groupsManager.ListenEvent<GroupAddedEvent>(this);

            _entitiesManager = entitiesManager ?? new EntitiesManager();
            _entitiesManager.ListenEvent<ComponentAddedEvent>(this);
            _entitiesManager.ListenEvent<ComponentRemovedEvent>(this);
            _entitiesManager.ListenEvent<ComponentUpdatedEvent>(this);
            _entitiesManager.ListenEvent<ComponentPreUpdatedEvent>(this);
            _entitiesManager.ListenEvent<DisposedEvent>(this);
        }

        #region GET NEW ENTITY

        public IEntity GetNewEntity()
        {
            return _entitiesManager.GetNewEntity();
        }

        public IEntity GetNewEntity(Action<IEntity> setup)
        {
            return _entitiesManager.GetNewEntity(setup);
        }

        #endregion

        #region GET GROUP

        public EntitiesGroup GetGroup(IMatcher matcher)
        {
            return _groupsManager.GetGroup(matcher);
        }

        public EntitiesGroup GetGroup(ushort componentId)
        {
            return _groupsManager.GetGroup(componentId);
        }

        public EntitiesGroup GetGroup(ushort[] componentIds)
        {
            return _groupsManager.GetGroup(componentIds);
        }

        #endregion

        #region CALLBACKS

        public void HandleCallback(ComponentAddedEvent arguments)
        {
            OnEntityComponentRemoved?.Invoke((IEntity)arguments.Holder, arguments.Value);
        }

        public void HandleCallback(ComponentRemovedEvent arguments)
        {
            OnEntityComponentRemoved?.Invoke((IEntity)arguments.Holder, arguments.Value);
        }

        public void HandleCallback(ComponentUpdatedEvent arguments)
        {
            OnEntityComponentUpdated?.Invoke((IEntity)arguments.Holder, arguments.Value);
        }

        public void HandleCallback(ComponentPreUpdatedEvent arguments)
        {
            OnEntityComponentPreUpdated?.Invoke
            (
                (IEntity)arguments.Holder,
                arguments.PreviousValue,
                arguments.FutureValue
            );
        }

        public void HandleCallback(DisposedEvent arguments)
        {
            OnEntityDisposed?.Invoke(arguments.Value as IEntity);
        }

        public void HandleCallback(GroupAddedEvent arguments)
        {
            foreach (var pair in _entitiesManager.Entities)
            {
                var entity = pair.Value;

                if (arguments.Matcher.IsMatch(entity))
                {
                    _groupsManager.AddToGroup(arguments.Value, entity);
                }
            }
        }

        #endregion
    }
}