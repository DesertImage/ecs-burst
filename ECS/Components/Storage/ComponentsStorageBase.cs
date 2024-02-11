using System;

namespace DesertImage.ECS
{
    public abstract class ComponentsStorageBase : IDisposable
    {
        public abstract void Dispose();
    }
}