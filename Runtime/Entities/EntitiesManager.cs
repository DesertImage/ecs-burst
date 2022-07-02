using System;
using System.Collections.Generic;
using DesertImage.Events;
using DesertImage.Pools;

namespace DesertImage.ECS
{
    public class EntitiesManager : EventUnit,
        IListen<ComponentAddedEvent>,
        IListen<ComponentRemovedEvent>,
        IListen<ComponentPreUpdatedEvent>,
        IListen<ComponentUpdatedEvent>,
        IListen<DisposedEvent>
    {
        public Dictionary<int, IEntity> Entities { get; } = new Dictionary<int, IEntity>();

        private readonly Pool<Entity> _pool = new Pool<Entity>();

        public IEntity GetEntityById(ushort id)
        {
            return Entities.TryGetValue(id, out var entity) ? entity : GetNewEntity();
        }

        public IEntity GetNewEntity()
        {
            var newEntity = _pool.GetInstance();

            newEntity.ListenEvent<ComponentAddedEvent>(this);
            newEntity.ListenEvent<ComponentRemovedEvent>(this);
            newEntity.ListenEvent<ComponentUpdatedEvent>(this);
            newEntity.ListenEvent<ComponentPreUpdatedEvent>(this);
            newEntity.ListenEvent<DisposedEvent>(this);

            Entities.Add(newEntity.Id, newEntity);

            return newEntity;
        }

        public IEntity GetNewEntity(Action<IEntity> setup)
        {
            var newEntity = GetNewEntity();

            setup?.Invoke(newEntity);

            return newEntity;
        }

        #region CALLBACKS

        public void HandleCallback(ComponentAddedEvent arguments)
        {
            EventsManager.Send(arguments);
        }

        public void HandleCallback(ComponentRemovedEvent arguments)
        {
            EventsManager.Send(arguments);
        }

        public void HandleCallback(ComponentPreUpdatedEvent arguments)
        {
            EventsManager.Send(arguments);
        }

        public void HandleCallback(ComponentUpdatedEvent arguments)
        {
            EventsManager.Send(arguments);
        }

        public void HandleCallback(DisposedEvent arguments)
        {
            var entity = (Entity)arguments.Value;

            // entity.UnlistenEvent<ComponentAddedEvent>(this);
            // entity.UnlistenEvent<ComponentRemovedEvent>(this);
            // entity.UnlistenEvent<ComponentUpdatedEvent>(this);
            // entity.UnlistenEvent<ComponentPreUpdatedEvent>(this);
            // entity.UnlistenEvent<DisposedEvent>(this);

            EventsManager.Send(arguments);
            
            _pool.ReturnInstance(entity);
        }

        #endregion
    }
}