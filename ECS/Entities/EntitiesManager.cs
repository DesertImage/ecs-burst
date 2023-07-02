using System;
using System.Collections.Generic;
using DesertImage.Pools;

namespace DesertImage.ECS
{
    public struct EntitiesManager
    {
        private readonly Pool<SortedSetPoolable<int>> _pool;
        private readonly Stack<Entity> _entitiesPool;

        private ComponentsStorageBase[] _componentsStorages;

        private static int _idCounter;

        private readonly WorldState _state;

        public EntitiesManager(WorldState state)
        {
            _state = state;

            _componentsStorages = new ComponentsStorageBase[ECSSettings.ComponentsDenseCapacity];

            _pool = new Pool<SortedSetPoolable<int>>();
            _entitiesPool = new Stack<Entity>();

            _idCounter = -1;
        }

        public readonly Entity GetEntityById(int id) =>
            _state.Entities.TryGetValue(id, out var entity) ? entity : GetNewEntity();

        public readonly Entity GetNewEntity()
        {
            var newEntity = _entitiesPool.Count > 0 ? _entitiesPool.Pop() : new Entity(++_idCounter);

            var id = newEntity.Id;

            _state.Entities.Add(id, newEntity);
            _state.Components.Add(id, _pool.GetInstance());

            return newEntity;
        }

        public readonly SortedSetPoolable<int> GetComponents(int id) => _state.Components[id];

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

            var storage = GetStorage<T>(componentId);

            storage.Data.Add(entityId, component);
            _state.Components[entityId].Add(componentId);

#if UNITY_EDITOR
            if (ComponentsDebug.Components.TryGetValue(entityId, out var components))
            {
                components[componentId] = component;
            }
            else
            {
                var array = new object[ECSSettings.ComponentsDenseCapacity];
                array[componentId] = component;
                ComponentsDebug.Components.Add(entityId, array);
            }
#endif
        }

        public void RemoveComponent<T>(int entityId) where T : struct
        {
#if DEBUG
            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
#endif
            var componentId = ComponentTools.GetComponentId<T>();

            var storage = GetStorage<T>(componentId);

            storage.Data.Remove(entityId);
            _state.Components[entityId].Remove(componentId);
            
#if UNITY_EDITOR
            if (ComponentsDebug.Components.TryGetValue(entityId, out var components))
            {
                components[componentId] = null;
            }
#endif
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
#else
                return false;
#endif
            }

            var storage = GetStorage<T>(componentId);

            return storage.Data.Contains(entityId);
        }

        public ref T GetComponent<T>(int entityId) where T : struct
        {
#if DEBUG
            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
            if (!HasComponent<T>(entityId)) throw new Exception($"Entity {entityId} has not component {typeof(T)}");
#endif
            var componentId = ComponentTools.GetComponentId<T>();
            var storage = (ComponentsStorage<T>)_componentsStorages[componentId];

            return ref storage.Data.Get(entityId);
        }

        public bool IsAlive(int entityId) => _state.Entities.ContainsKey(entityId);

        public void DestroyEntity(int entityId)
        {
            var components = GetComponents(entityId);
            components.Clear();

            var entity = GetEntityById(entityId);

            _state.Components.Remove(entityId);
            _state.Entities.Remove(entityId);

            _pool.ReturnInstance(components);

            _entitiesPool.Push(entity);
        }

        private ComponentsStorage<T> GetStorage<T>(int componentId)
        {
            var storage = (ComponentsStorage<T>)_componentsStorages[componentId];

            if (storage != null) return storage;

            var newInstance = new ComponentsStorage<T>
            (
                ECSSettings.ComponentsDenseCapacity,
                ECSSettings.ComponentsSparseCapacity
            );

            _componentsStorages[componentId] = newInstance;
            storage = newInstance;

            return storage;
        }
    }
}