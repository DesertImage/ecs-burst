using System;
using DesertImage.Events;

namespace DesertImage.ECS
{
    public interface IComponentHolder : IEventUnit, IDisposable
    {
        event Action<IComponentHolder, IComponent> OnComponentAdded;
        event Action<IComponentHolder, IComponent> OnComponentRemoved;
        event Action<IComponentHolder, IComponent, IComponent> OnComponentPreUpdated;
        event Action<IComponentHolder, IComponent> OnComponentUpdated;

        event Action<IComponentHolder> OnDispose;

        IComponent Add(IComponent component);
        TComponent Add<TComponent>() where TComponent : IComponent, new();

        IComponent Get(ushort id);
        T Get<T>(ushort id) where T : IComponent;

        bool HasComponent(ushort id);

        void Remove(ushort id);
    }
}