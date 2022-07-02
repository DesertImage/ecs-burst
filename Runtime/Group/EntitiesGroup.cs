using System.Collections.Generic;
using DesertImage.Events;

namespace DesertImage.ECS
{
    public class EntitiesGroup : EventUnit, IPoolable
    {
        public ushort Id { get; }

        private static ushort _groupsIdCounter;

        public List<IEntity> Entities { get; }

        private readonly HashSet<IEntity> _entitiesHashSet = new HashSet<IEntity>();

        private bool _isDisposing;

        public EntitiesGroup()
        {
            Id = _groupsIdCounter;

            Entities = new List<IEntity>();

            _groupsIdCounter++;
        }

        public EntitiesGroup(List<IEntity> entities)
        {
            Id = _groupsIdCounter;

            Entities = entities;

            _groupsIdCounter++;
        }

        public void Add(IEntity entity)
        {
            _entitiesHashSet.Add(entity);

            Entities.Add(entity);

            EventsManager.Send
            (
                new EntityAddedEvent
                {
                    Group = this,
                    Value = entity
                }
            );
        }

        public void Remove(IEntity entity)
        {
            if (!Contains(entity)) return;

            if (!_isDisposing)
            {
                _entitiesHashSet.Remove(entity);
                Entities.Remove(entity);
            }

            EventsManager.Send
            (
                new EntityRemovedEvent
                {
                    Group = this,
                    Value = entity
                }
            );
        }

        public bool Contains(IEntity entity)
        {
            return _entitiesHashSet.Contains(entity);
        }

        public void OnCreate()
        {
        }

        public void ReturnToPool()
        {
            Dispose();
        }

        public void PreUpdate(IEntity entity, IComponent component, IComponent newValues)
        {
            EventsManager.Send
            (
                new EntityPreUpdatedEvent
                {
                    Value = entity,
                    Previous = component,
                    Future = newValues,
                    Group = this
                }
            );
        }

        public void Update(IEntity entity, IComponent component)
        {
            EventsManager.Send
            (
                new EntityUpdatedEvent
                {
                    Value = entity,
                    Component = component,
                    Group = this
                }
            );
        }

        public void Dispose()
        {
            _isDisposing = true;

            foreach (var entity in Entities)
            {
                Remove(entity);
            }

            _entitiesHashSet.Clear();
            Entities.Clear();

            _isDisposing = false;
        }
    }
}