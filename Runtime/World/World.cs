using System;
using System.Collections.Generic;
using DesertImage.Events;
using DesertImage.Extensions;
using DesertImage.Pools;
using External;
using Group;

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

        HashSet<IEntity> Entities { get; }

        void AddEntity(IEntity entity);
        void RemoveEntity(IEntity entity);

        IEntity GetNewEntity();
        IEntity GetNewEntity(Action<IEntity> setup);

        EntityGroup GetGroup(IMatcher matcher);
        EntityGroup GetGroup(ushort componentId);
        EntityGroup GetGroup(ushort[] componentIds);
    }

    public class World : IWorld, IListen<ComponentAddedEvent>,
        IListen<ComponentRemovedEvent>,
        IListen<ComponentPreUpdatedEvent>,
        IListen<ComponentUpdatedEvent>
    {
        public event Action<IEntity> OnEntityAdded;
        public event Action<IEntity> OnEntityRemoved;

        public event Action<IEntity, IComponent> OnEntityComponentAdded;
        public event Action<IEntity, IComponent> OnEntityComponentRemoved;
        public event Action<IEntity, IComponent, IComponent> OnEntityComponentPreUpdated;
        public event Action<IEntity, IComponent> OnEntityComponentUpdated;

        public HashSet<IEntity> Entities { get; } = new HashSet<IEntity>();


        private GroupsManager _groupsManager;

        private readonly Pool<IEntity> EntitiesPool = new PoolEntity();

        public World(GroupsManager groupsManager)
        {
            _groupsManager = groupsManager;
        }

        public void AddEntity(IEntity entity)
        {
            Entities.Add(entity);

            entity.ListenEvent<ComponentAddedEvent>(this);
            entity.ListenEvent<ComponentRemovedEvent>(this);
            entity.ListenEvent<ComponentPreUpdatedEvent>(this);
            entity.ListenEvent<ComponentUpdatedEvent>(this);

            OnEntityAdded?.Invoke(entity);
        }

        public void RemoveEntity(IEntity entity)
        {
            if (!Entities.Contains(entity)) return;

            entity.UnlistenEvent<ComponentAddedEvent>(this);
            entity.UnlistenEvent<ComponentRemovedEvent>(this);
            entity.UnlistenEvent<ComponentPreUpdatedEvent>(this);
            entity.UnlistenEvent<ComponentUpdatedEvent>(this);

            Entities.Remove(entity);

            OnEntityRemoved?.Invoke(entity);
        }

        public IEntity GetNewEntity()
        {
            var entity = GetEntity();

            AddEntity(entity);

            return entity;
        }

        public IEntity GetNewEntity(Action<IEntity> setup)
        {
            var entity = GetEntity();

            AddEntity(entity);

            setup?.Invoke(entity);

            return entity;
        }

        private IEntity GetEntity()
        {
            var entity = EntitiesPool.GetInstance();

            entity.OnDispose += EntityOnDispose;

            return entity;
        }

        public EntityGroup GetGroup(IMatcher matcher)
        {
            return _groupsManager.GetGroup(matcher);
        }

        public EntityGroup GetGroup(ushort componentId)
        {
            return _groupsManager.GetGroup(componentId);
        }

        public EntityGroup GetGroup(ushort[] componentIds)
        {
            return _groupsManager.GetGroup(componentIds);
        }

        #region CALLBACKS

        private void EntityOnComponentRemoved(IComponentHolder componentHolder, IComponent component)
        {
            OnEntityComponentAdded?.Invoke((IEntity)componentHolder, component);
        }

        private void EntityOnComponentAdded(IComponentHolder componentHolder, IComponent component)
        {
            OnEntityComponentRemoved?.Invoke((IEntity)componentHolder, component);
        }

        private void EntityOnPreUpdated(IComponentHolder componentHolder, IComponent previous, IComponent future)
        {
            OnEntityComponentPreUpdated?.Invoke((IEntity)componentHolder, previous, future);
        }

        private void EntityOnUpdated(IComponentHolder componentHolder, IComponent component)
        {
            OnEntityComponentUpdated?.Invoke((IEntity)componentHolder, component);
        }

        private void EntityOnDispose(IComponentHolder componentHolder)
        {
            var entity = componentHolder as IEntity;

            // ReSharper disable once PossibleNullReferenceException
            entity.OnDispose -= EntityOnDispose;

            RemoveEntity(entity);

            EntitiesPool.ReturnInstance(entity);
        }

        public void HandleCallback(ComponentAddedEvent arguments)
        {
            EntityOnComponentAdded(arguments.Holder, arguments.Value);
        }

        public void HandleCallback(ComponentRemovedEvent arguments)
        {
            EntityOnComponentRemoved(arguments.Holder, arguments.Value);
        }

        public void HandleCallback(ComponentPreUpdatedEvent arguments)
        {
            EntityOnPreUpdated(arguments.Holder, arguments.PreviousValue, arguments.FutureValue);
        }

        public void HandleCallback(ComponentUpdatedEvent arguments)
        {
            EntityOnUpdated(arguments.Holder, arguments.Value);
        }

        #endregion
    }
}