using System.Collections.Generic;
using UnityEngine;

namespace DesertImage.Assets
{
    public partial class SpawnManager
    {
        private readonly Dictionary<uint, Object> _storage = new Dictionary<uint, Object>();

        private readonly Dictionary<uint, Dictionary<uint, Object>> _componentsCache =
            new Dictionary<uint, Dictionary<uint, Object>>();

        public void Register(uint id, GameObject obj) => _storage[id] = obj;

        public GameObject Spawn(uint id)
        {
            return _storage.TryGetValue(id, out var obj) ? (GameObject)obj : default;
        }

        public T SpawnAs<T>(uint id) where T : Component
        {
            var obj = Spawn(id);
            T target;
            var componentId = ObjectTypeCounter.GetId<T>();

            if (_componentsCache.TryGetValue(id, out var dictionary))
            {
                if (dictionary.TryGetValue(componentId, out var cached))
                {
                    target = (T)cached;
                }
                else
                {
                    target = obj.GetComponent<T>();
                    dictionary[componentId] = target;
                }
            }
            else
            {
                var cacheDictionary = new Dictionary<uint, Object>();

                target = obj.GetComponent<T>();
                cacheDictionary[componentId] = target;

                _componentsCache.Add(id, cacheDictionary);
            }

            return target;
        }
    }
}