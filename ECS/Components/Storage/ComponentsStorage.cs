using System;
using DesertImage.Collections;

namespace DesertImage.ECS
{
    public class ComponentsStorage<T> : ComponentsStorageBase, IDisposable where T : unmanaged
    {
        public UnsafeSparseSet<T> Data;

        public ComponentsStorage(int denseCapacity, int sparseCapacity)
        {
            Data = new UnsafeSparseSet<T>(denseCapacity, sparseCapacity);
        }

        public override void Dispose() => Data.Dispose();
    }
}