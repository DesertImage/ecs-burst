using System;
using System.Collections.Generic;
using DesertImage.Pools;

namespace DesertImage.ECS
{
    public struct EntitiesManager
    {
        private readonly Dictionary<int, Entity> _entities;
        private readonly Dictionary<int, SortedSetPoolable<int>> _components;

        private readonly Pool<SortedSetPoolable<int>> _pool;
        private readonly Stack<Entity> _entitiesPool;

        private ComponentsStorageBase[] _componentsStorages;

        private static int _idCounter;

        public EntitiesManager(World world)
        {
            _entities = new Dictionary<int, Entity>();
            _components = new Dictionary<int, SortedSetPoolable<int>>();
            _componentsStorages = new ComponentsStorageBase[ECSSettings.ComponentsDenseCapacity];

            _pool = new Pool<SortedSetPoolable<int>>();
            _entitiesPool = new Stack<Entity>();

            _idCounter = -1;
        }

        public Entity GetEntityById(int id) => _entities.TryGetValue(id, out var entity) ? entity : GetNewEntity();

        public Entity GetNewEntity()
        {
            var newEntity = _entitiesPool.Count > 0 ? _entitiesPool.Pop() : new Entity(++_idCounter);

            var id = newEntity.Id;

            _entities.Add(id, newEntity);
            _components.Add(id, _pool.GetInstance());

            return newEntity;
        }

        public SortedSetPoolable<int> GetComponents(int id) => _components[id];

        public void ReplaceComponent<T>(int entityId, T component) where T : struct
        {
#if DEBUG
            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
#endif
            var componentId = ComponentTools.GetComponentId<T>();

            if (componentId >= _componentsStorages.Length)
            {
                Array.Resize(ref _componentsStorages, componentId << 1);
            }

            var storage = (ComponentsStorage<T>)_componentsStorages[componentId];

            if (storage == null)
            {
                var newInstance = new ComponentsStorage<T>
                (
                    ECSSettings.ComponentsDenseCapacity,
                    ECSSettings.ComponentsSparseCapacity
                );

                _componentsStorages[componentId] = newInstance;
                storage = newInstance;
            }

            storage.Data.Add(entityId, component);
            _components[entityId].Add(componentId);
        }

        public void RemoveComponent<T>(int entityId) where T : struct
        {
#if DEBUG
            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
#endif
            var componentId = ComponentTools.GetComponentId<T>();

            var storage = (ComponentsStorage<T>)_componentsStorages[componentId];

            storage.Data.Remove(entityId);
            _components[entityId].Remove(componentId);
        }

        public bool HasComponent<T>(int entityId) where T : struct
        {
#if DEBUG
            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
#endif
            var componentId = ComponentTools.GetComponentId<T>();

            if (componentId >= _componentsStorages.Length)
            {
#if DEBUG
                throw new Exception("out of ComponentStorages");
#endif
                return false;
            }

            var storage = (ComponentsStorage<T>)_componentsStorages[componentId];

            return storage.Data.Contains(entityId);
        }

        public ref T GetComponent<T>(int entityId) where T : struct
        {
#if DEBUG
            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
#endif
            var componentId = ComponentTools.GetComponentId<T>();
            var storage = (ComponentsStorage<T>)_componentsStorages[componentId];

            return ref storage.Data.Get(entityId);
        }

        public bool IsAlive(int entityId) => _entities.ContainsKey(entityId);

        public void DestroyEntity(int entityId)
        {
            var components = GetComponents(entityId);
            components.Clear();

            var entity = GetEntityById(entityId);

            _components.Remove(entityId);
            _entities.Remove(entityId);

            _pool.ReturnInstance(components);

            _entitiesPool.Push(entity);
        }
    }
}