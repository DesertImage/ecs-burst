using System;
using System.Collections.Generic;
using DesertImage.Pools;

namespace DesertImage.ECS
{
    public struct EntitiesManager
    {
        private readonly Pool<SortedSetPoolable<int>> _pool;
        private readonly Stack<Entity> _entitiesPool;

        private static int _idCounter;

        private WorldState _state;

        public EntitiesManager(WorldState state)
        {
            _state = state;

            _pool = new Pool<SortedSetPoolable<int>>();
            _entitiesPool = new Stack<Entity>();

            _idCounter = -1;
        }

        public readonly Entity GetEntityById(int id)
        {
            return _state.Entities.TryGetValue(id, out var entity) ? entity : GetNewEntity();
        }

        public readonly Entity GetNewEntity()
        {
            var newEntity = _entitiesPool.Count > 0 ? _entitiesPool.Pop() : new Entity(++_idCounter);

            var id = newEntity.Id;

            _state.Entities.Add(id, newEntity);
            _state.EntityComponents.Add(id, _pool.GetInstance());

            return newEntity;
        }

        public readonly SortedSetPoolable<int> GetComponents(int id) => _state.EntityComponents[id];

        public void ReplaceComponent<T>(int entityId, in T component) where T : struct
        {
#if DEBUG
            if (component is ISharedComponent)
            {
                throw new Exception($"Attempt to replace shared component {typeof(T).Name} as NON shared component");
            }

            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
#endif
            var componentId = ComponentTools.GetComponentId<T>();

            if (componentId >= _state.Storages.Length)
            {
                Array.Resize(ref _state.Storages, componentId << 1);
            }

            var storage = GetStorage<T>(componentId);

            storage.Data.Add(entityId, component);

            var entityComponent = _state.EntityComponents[entityId];
            entityComponent.Add(componentId);

#if UNITY_EDITOR
            AddEditorComponent(entityId, componentId, component);
#endif
        }

        public void ReplaceSharedComponent<T>(int entityId, in T component) where T : struct, ISharedComponent
        {
#if DEBUG
            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
#endif
            var componentId = ComponentTools.GetComponentId<T>();

            if (componentId >= _state.Storages.Length)
            {
                Array.Resize(ref _state.Storages, componentId << 1);
            }

            var storage = GetSharedStorage<T>(componentId);

            storage.Data = component;
            storage.Entities.Add(entityId);

            var entityComponents = _state.EntityComponents[entityId];
            entityComponents.Add(componentId);

#if UNITY_EDITOR
            AddEditorComponent(entityId, componentId, component);
#endif
        }

        public void ReplaceStaticComponent<T>(int entityId, in T component) where T : struct, IStaticComponent
        {
#if DEBUG
            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
#endif
            var componentId = ComponentTools.GetComponentId<T>();

            if (componentId >= _state.Storages.Length)
            {
                Array.Resize(ref _state.Storages, componentId << 1);
            }

            var storage = GetStaticStorage<T>(componentId);

            storage.Data = component;

            var entityComponents = _state.EntityComponents[entityId];
            entityComponents.Add(componentId);

#if UNITY_EDITOR
            AddEditorComponent(entityId, componentId, component);
#endif
        }

        public void RemoveComponent<T>(int entityId) where T : struct
        {
#if DEBUG
            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
            if (!HasComponent<T>(entityId)) throw new Exception($"Entity {entityId} has not {typeof(T).Name}!");
#endif
            var componentId = ComponentTools.GetComponentId<T>();

#if DEBUG
            if (IsSharedComponent<T>(componentId))
            {
                throw new Exception($"Attempt to remove shared component {typeof(T).Name} by NON shared method");
            }
#endif

            GetStorage<T>(componentId).Data.Remove(entityId);

            var entityComponents = _state.EntityComponents[entityId];
            entityComponents.Remove(componentId);

#if UNITY_EDITOR
            RemoveEditorComponent<T>(entityId, componentId);
#endif

            if (entityComponents.Count == 0)
            {
                DestroyEntity(entityId);
            }
        }

        public void RemoveSharedComponent<T>(int entityId) where T : struct, ISharedComponent
        {
#if DEBUG
            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
            if (!HasSharedComponent<T>(entityId)) throw new Exception($"Entity {entityId} has not {typeof(T).Name}!");
#endif
            var componentId = ComponentTools.GetComponentId<T>();

            var sharedStorage = (ComponentsSharedStorage<T>)_state.SharedStorages[componentId];
            sharedStorage.Entities.Remove(entityId);

            var entityComponents = _state.EntityComponents[entityId];
            entityComponents.Remove(componentId);

#if UNITY_EDITOR
            RemoveEditorComponent<T>(entityId, componentId);
#endif

            if (entityComponents.Count == 0)
            {
                DestroyEntity(entityId);
            }
        }

        public bool HasComponent<T>(int entityId) where T : struct
        {
#if DEBUG
            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
#endif
            var componentId = ComponentTools.GetComponentId<T>();

            if (componentId >= _state.Storages.Length)
            {
#if DEBUG
                throw new Exception("out of ComponentStorages");
#else
                return false;
#endif
            }

            return GetStorage<T>(componentId).Data.Contains(entityId);
        }

        public bool HasSharedComponent<T>(int entityId) where T : struct, ISharedComponent
        {
#if DEBUG
            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
#endif
            var componentId = ComponentTools.GetComponentId<T>();

            if (componentId >= _state.SharedStorages.Length)
            {
#if DEBUG
                throw new Exception("out of ComponentStorages");
#else
                return false;
#endif
            }

            var storage = (ComponentsSharedStorage<T>)_state.SharedStorages[componentId];
            return storage?.Entities.Contains(entityId) ?? false;
        }
        
        public bool HasStaticComponent<T>(int entityId) where T : struct, IStaticComponent
        {
#if DEBUG
            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
#endif
            var componentId = ComponentTools.GetComponentId<T>();

            if (componentId >= _state.StaticStorages.Length)
            {
#if DEBUG
                throw new Exception("out of ComponentStorages");
#else
                return false;
#endif
            }

            var storage = (ComponentsStaticStorage<T>)_state.StaticStorages[componentId];
            return storage != null;
        }

        public ref T GetComponent<T>(int entityId) where T : struct
        {
#if DEBUG
            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
            if (!HasComponent<T>(entityId)) throw new Exception($"Entity {entityId} has not component {typeof(T)}");
#endif
            var componentId = ComponentTools.GetComponentId<T>();
#if DEBUG
            if (IsSharedComponent<T>(componentId))
            {
                throw new Exception($"Attempt to get shared component {typeof(T).Name} by NON shared method");
            }
#endif
            var storage = (ComponentsStorage<T>)_state.Storages[componentId];
            return ref storage.Data.Get(entityId);
        }

        public ref T GetSharedComponent<T>(int entityId) where T : struct, ISharedComponent
        {
#if DEBUG
            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
            if (!HasSharedComponent<T>(entityId))
                throw new Exception($"Entity {entityId} has not component {typeof(T)}");
#endif
            var componentId = ComponentTools.GetComponentId<T>();

            var sharedStorage = (ComponentsSharedStorage<T>)_state.SharedStorages[componentId];
            return ref sharedStorage.Data;
        }

        public ref T GetStaticComponent<T>(int entityId) where T : struct, IStaticComponent
        {
#if DEBUG
            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
#endif
            var componentId = ComponentTools.GetComponentId<T>();

            var sharedStorage = (ComponentsStaticStorage<T>)_state.StaticStorages[componentId];
            return ref sharedStorage.Data;
        }

        public readonly bool IsAlive(int entityId) => _state.Entities.ContainsKey(entityId);

        public void DestroyEntity(int entityId)
        {
            var components = GetComponents(entityId);
            components.Clear();

            var entity = GetEntityById(entityId);

            _state.EntityComponents.Remove(entityId);
            _state.Entities.Remove(entityId);

            _pool.ReturnInstance(components);

            _entitiesPool.Push(entity);
        }

        #region STORAGES

        private ComponentsStorage<T> GetStorage<T>(int componentId)
        {
            var storage = (ComponentsStorage<T>)_state.Storages[componentId];

            if (storage != null) return storage;

            var newInstance = new ComponentsStorage<T>
            (
                ECSSettings.ComponentsDenseCapacity,
                ECSSettings.ComponentsSparseCapacity
            );

            _state.Storages[componentId] = newInstance;
            storage = newInstance;

            return storage;
        }

        private ComponentsSharedStorage<T> GetSharedStorage<T>(int componentId)
        {
            var storage = (ComponentsSharedStorage<T>)_state.SharedStorages[componentId];

            if (storage != null) return storage;

            var newInstance = new ComponentsSharedStorage<T>
            (
                new HashSet<int>()
            );

            _state.SharedStorages[componentId] = newInstance;
            storage = newInstance;

            return storage;
        }

        private ComponentsStaticStorage<T> GetStaticStorage<T>(int componentId)
        {
            var storage = (ComponentsStaticStorage<T>)_state.SharedStorages[componentId];

            if (storage != null) return storage;

            var newInstance = new ComponentsStaticStorage<T>();

            _state.StaticStorages[componentId] = newInstance;
            storage = newInstance;

            return storage;
        }

        #endregion

#if UNITY_EDITOR
        private static void AddEditorComponent<T>(int entityId, int componentId, T component)
        {
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
        }

        private static void RemoveEditorComponent<T>(int entityId, int componentId)
        {
            if (ComponentsDebug.Components.TryGetValue(entityId, out var components))
            {
                components[componentId] = null;
            }
        }
#endif

        private bool IsSharedComponent<T>(int componentId) => _state.SharedStorages[componentId] != null;
    }
}