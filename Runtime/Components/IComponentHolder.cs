using System;
using DesertImage.Events;

namespace DesertImage.ECS
{
    public interface IComponentHolder : IEventUnit, IDisposable
    {
        IComponent Add(IComponent component);
        TComponent Add<TComponent>() where TComponent : IComponent, new();

        IComponent Get(ushort id);
        T Get<T>(ushort id) where T : IComponent;

        bool HasComponent(ushort id);

        void Remove(ushort id);
    }
}