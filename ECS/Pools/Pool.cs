using System;
using Unity.Collections;

namespace DesertImage.Pools
{
    public struct Pool<T> where T : struct, IPoolable, IDisposable
    {
        private NativeQueue<T> Queue;

        public Pool(NativeQueue<T> queue)
        {
            Queue = new NativeQueue<T>(AllocatorManager.Persistent);
        }

        public void Register(int count)
        {
            for (var i = 0; i < count; i++)
            {
                ReturnInstance(CreateInstance());
            }
        }

        public T GetInstance()
        {
            var instance = Queue.Count > 0 ? Queue.Dequeue() : CreateInstance();
            instance.OnCreate();
            return instance;
        }

        public void ReturnInstance(T instance) => Queue.Enqueue(instance);

        private T CreateInstance() => new T();

        public void Dispose() => Queue.Dispose();
    }
}