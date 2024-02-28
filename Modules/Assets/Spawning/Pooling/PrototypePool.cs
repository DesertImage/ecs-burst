using System;
using System.Collections.Generic;

namespace DesertImage.Assets
{
    public class PrototypePool<T> : IPrototypePool<T> where T : IPoolable
    {
        private struct PoolData
        {
            public T Prototype;
            public Stack<T> Stack;
        }

        private readonly Dictionary<uint, PoolData> _data = new Dictionary<uint, PoolData>();
        private readonly Func<T, T> _factory;

        public PrototypePool(Func<T, T> factory)
        {
            _factory = factory;
        }

        public void Register(uint id, T instance, int preRegisteredCount)
        {
            if (!_data.TryGetValue(id, out var poolData))
            {
                poolData = _data[id] = new PoolData
                {
                    Prototype = instance,
                    Stack = new Stack<T>()
                };
            }

            for (var i = 0; i < preRegisteredCount; i++)
            {
                poolData.Stack.Push(GetNew(instance));
            }
        }

        public T Get(uint id)
        {
            if (!_data.TryGetValue(id, out var poolData)) return default;
            var stack = poolData.Stack;
            var instance = stack.Count > 0 ? stack.Pop() : GetNew(poolData.Prototype);

            instance.OnUnpool(id);

            return instance;
        }

        public void Release(T instance)
        {
            var id = instance.PoolableId;

            if (!_data.TryGetValue(id, out var poolData))
            {
#if DEBUG
                throw new Exception($"{id} not registered in pool");
#else
                return;
#endif
            }

            instance.OnRelease();
            poolData.Stack.Push(instance);
        }

        public bool ContainsId(uint id) => _data.ContainsKey(id);
        public void Clear() => _data.Clear();

        private T GetNew(T prototype) => _factory.Invoke(prototype);
    }
}