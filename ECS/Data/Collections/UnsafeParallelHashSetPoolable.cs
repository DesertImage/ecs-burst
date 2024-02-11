using System;
using DesertImage;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace DesertImage.ECS.Data
{
    public struct UnsafeParallelHashSetPoolable<T> : IPoolable, INativeDisposable where T : unmanaged, IEquatable<T>
    {
        public UnsafeParallelHashSet<T> Collection => _collection;

        private UnsafeParallelHashSet<T> _collection;

        public UnsafeParallelHashSetPoolable(int capacity, AllocatorManager.AllocatorHandle allocator)
        {
            _collection = new UnsafeParallelHashSet<T>(10, AllocatorManager.Persistent);
        }

        public void OnCreate()
        {
            if (_collection.IsCreated) return;
            _collection = new UnsafeParallelHashSet<T>(10, AllocatorManager.Persistent);
        }

        public void ReturnToPool() => _collection.Clear();

        public int Count() => _collection.Count();

        public void Add(T instance) => _collection.Add(instance);
        public void Remove(T instance) => _collection.Remove(instance);
        public bool Contains(T instance) => _collection.Contains(instance);

        public void Clear() => _collection.Clear();

        public void Dispose() => _collection.Dispose();
        public JobHandle Dispose(JobHandle inputDeps) => _collection.Dispose(inputDeps);
    }
}