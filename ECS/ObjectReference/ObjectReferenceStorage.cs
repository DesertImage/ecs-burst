using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace DesertImage.ECS
{
    public struct ObjectReferenceStorage
    {
        public readonly bool IsNotNull;

        private readonly Dictionary<int, uint> _instanceIdToId;
        private Object[] _objects;
        private uint _indexCounter;

        public ObjectReferenceStorage(int capacity)
        {
            _instanceIdToId = new Dictionary<int, uint>(capacity);
            _objects = new Object[capacity];
            _indexCounter = 0;

            IsNotNull = true;
        }

        public T Get<T>(ref uint id, Object obj) where T : Object
        {
            if (id != 0) return (T)_objects[id - 1];

            if (obj)
            {
                var instanceId = obj.GetInstanceID();
                if (_instanceIdToId.TryGetValue(instanceId, out var index))
                {
                    id = index;
                }
                else
                {
                    Register(ref id, obj);
                    _instanceIdToId.Add(instanceId, id);
                }
            }
            else
            {
                return default;
            }

            return (T)_objects[id - 1];
        }

        private void Register(ref uint id, Object obj)
        {
            var index = ++_indexCounter - 1;

            if (index >= _objects.Length)
            {
                Array.Resize(ref _objects, _objects.Length << 1);
            }
            else
            {
                if (_objects[index])
                {
#if DEBUG_MODE
                    throw new Exception($"Storage already contains {obj}");
#else
                 return;
#endif
                }
            }

            id = index + 1;
            _objects[index] = obj;
        }
    }
}