using System;
using DesertImage.Events;

namespace DesertImage.ECS
{
    public interface IComponent : IPoolable, IEventUnit
    {
        ushort Id { get; }

        void PreUpdated(IComponent component);
        void Updated();
    }

    [Serializable]
    public class Component<T> : EventUnit, IComponent, IDisposable where T : Component<T>
    {
        public virtual ushort Id { get; }

        public void Dispose()
        {
            ReturnToPool();
        }

        public virtual void OnCreate()
        {
        }

        public virtual void ReturnToPool()
        {
        }

        protected Component<T> GetInstanceFromPool()
        {
            return ComponentsTool.GetInstanceFromPool<Component<T>>();
        }

        public virtual Component<T> CopyTo(Component<T> component)
        {
            return component;
        }

        public void PreUpdated(IComponent component)
        {
            EventsManager.Send(new ComponentPreUpdatedEvent
            {
                PreviousValue = this,
                FutureValue = component
            });
        }

        public void Updated()
        {
            EventsManager.Send(new ComponentUpdatedEvent { Value = this });
        }
    }
}