using DesertImage.Events;

namespace DesertImage.ECS
{
    public interface IEntity : IComponentHolder, IPoolable, IEventUnit
    {
        int Id { get; }

        IComponent[] Components { get; }
    }

    public class Entity : EventUnit, IEntity,
        IListen<ComponentRemovedEvent>,
        IListen<ComponentPreUpdatedEvent>,
        IListen<ComponentUpdatedEvent>
    {
        public int Id { get; }

        public IComponent[] Components { get; }

        private int _componentsCount;

        private static int _entitiesIdCounter;

        public Entity()
        {
            Id = _entitiesIdCounter;
            _entitiesIdCounter++;

            Components = new IComponent[ECSSettings.ComponentsCount];
        }

        public Entity(int id, int componentsBuffer = ECSSettings.ComponentsCount)
        {
            Id = id;

            Components = new IComponent[componentsBuffer];
        }

        #region COMPONENTS

        public IComponent Add(IComponent component)
        {
            if (Components[component.Id] == null)
            {
                _componentsCount++;
            }

            Components[component.Id] = component;

            EventsManager.Send(new ComponentAddedEvent
            {
                Holder = this,
                Value = component
            });

            component.ListenEvent<ComponentPreUpdatedEvent>(this);
            component.ListenEvent<ComponentUpdatedEvent>(this);

            return component;
        }

        public TComponent Add<TComponent>() where TComponent : IComponent, new()
        {
            var component = ComponentsTool.GetInstanceFromPool<TComponent>();

            return (TComponent)Add(component);
        }

        public void Remove(ushort id)
        {
            var component = Components[id];

            if (component == null) return;

            Components[id] = null;

            component.UnlistenEvent<ComponentPreUpdatedEvent>(this);
            component.UnlistenEvent<ComponentUpdatedEvent>(this);

            EventsManager.Send(new ComponentRemovedEvent
            {
                Holder = this,
                Value = component
            });

            component.ReturnToPool();

            _componentsCount--;

            if (_componentsCount == 0)
            {
                ReturnToPool();
            }
        }

        public IComponent Get(ushort id)
        {
            return Components[id];
        }

        public T Get<T>(ushort id) where T : IComponent
        {
            return (T)Get(id);
        }

        public bool HasComponent(ushort id)
        {
            return Components[id] != null;
        }

        private void ClearComponents()
        {
            for (var i = 0; i < Components.Length; i++)
            {
                Components[i]?.ReturnToPool();
                Components[i] = null;
            }

            _componentsCount = 0;
        }

        #endregion

        public void OnCreate()
        {
        }

        public void ReturnToPool()
        {
            Dispose();
        }

        public void Dispose()
        {
            ClearComponents();

            EventsManager.Send(new DisposedEvent { Value = this });
            EventsManager.Clear();
        }

        #region CALLBACKS

        public void HandleCallback(ComponentRemovedEvent arguments)
        {
            EventsManager.Send
            (
                new ComponentRemovedEvent
                {
                    Holder = this,
                    Value = arguments.Value,
                }
            );
        }

        public void HandleCallback(ComponentPreUpdatedEvent arguments)
        {
            EventsManager.Send
            (
                new ComponentPreUpdatedEvent
                {
                    Holder = this,
                    PreviousValue = arguments.PreviousValue,
                    FutureValue = arguments.FutureValue
                }
            );
        }

        public void HandleCallback(ComponentUpdatedEvent arguments)
        {
            EventsManager.Send
            (
                new ComponentUpdatedEvent
                {
                    Holder = this,
                    Value = arguments.Value,
                }
            );
        }

        #endregion

        public override int GetHashCode()
        {
            return Id;
        }
    }
}