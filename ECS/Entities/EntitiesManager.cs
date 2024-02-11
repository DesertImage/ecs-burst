using System;
using DesertImage.Collections;
using DesertImage.ECS;
using Unity.Collections;

namespace DesertImage.ECS
{
    public struct EntitiesManager : IDisposable
    {
        private UnsafeQueue<UnsafeSparseSet<int>> _pool;
        private UnsafeQueue<Entity> _entitiesPool;

        private static int _idCounter;

        private WorldState _state;

        private int _lockIndex;

        public EntitiesManager(WorldState state)
        {
            _state = state;

            _pool = new UnsafeQueue<UnsafeSparseSet<int>>(50, Allocator.Persistent);
            _entitiesPool = new UnsafeQueue<Entity>(50, Allocator.Persistent);

            _idCounter = -1;
            _lockIndex = default;
        }

        public Entity GetEntityById(int id)
        {
            var entity = _state.Entities[id];
            return !entity.IsAlive ? GetNewEntity() : entity;
        }

        public Entity GetNewEntity()
        {
            _lockIndex.Lock();

            var newEntity = _entitiesPool.Count > 0 ? _entitiesPool.Dequeue() : new Entity(++_idCounter);

            var id = newEntity.Id;

            _state.Entities[id] = newEntity;
            _state.EntityComponents.Add(id, GetComponentsInstance());

            _lockIndex.Unlock();

            return newEntity;
        }

        public readonly UnsafeSparseSet<int> GetComponents(int id) => _state.EntityComponents[id];

        public void ReplaceComponent<T>(int entityId, in T component) where T : unmanaged
        {
#if DEBUG
            if (component is ISharedComponent)
            {
                throw new Exception($"Attempt to replace shared component {typeof(T).Name} as NON shared component");
            }

            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
#endif
            var componentId = ComponentTools.GetComponentId<T>();

            // if (componentId >= _state.Storages.Length)
            // {
            // Array.Resize(ref _state.Storages, componentId << 1);
            // }

            // var storage = GetStorage<T>(componentId);

            _state.Components.Write(componentId, entityId, component);
            // storage.Data.Add(entityId, component);

            var entityComponent = _state.EntityComponents[entityId];
            entityComponent.Add(entityId, componentId);

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

            // if (componentId >= _state.Storages.Length)
            // {
            // Array.Resize(ref _state.Storages, componentId << 1);
            // }

            var storage = GetSharedStorage<T>(componentId);

            storage.Data = component;
            storage.Entities.Add(entityId);

            var entityComponents = _state.EntityComponents[entityId];
            entityComponents.Add(entityId, componentId);

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

            // if (componentId >= _state.Storages.Length)
            // {
            // Array.Resize(ref _state.Storages, componentId << 1);
            // }

            var storage = GetStaticStorage<T>(componentId);

            storage.Data = component;

            var entityComponents = _state.EntityComponents[entityId];
            entityComponents.Add(entityId, componentId);

#if UNITY_EDITOR
            AddEditorComponent(entityId, componentId, component);
#endif
        }

        public void RemoveComponent<T>(int entityId) where T : unmanaged
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

            _state.Components.Write(componentId, entityId, default(T));
            // GetStorage<T>(componentId).Data.Remove(entityId);

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

            // var sharedStorage = (ComponentsSharedStorage<T>)_state.SharedStorages[componentId];
            // sharedStorage.Entities.Remove(entityId);

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

        public bool HasComponent<T>(int entityId) where T : unmanaged
        {
#if DEBUG
            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
#endif
            return _state.Components.Contains<T>(entityId);
        }

        public bool HasSharedComponent<T>(int entityId) where T : struct, ISharedComponent
        {
#if DEBUG
            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
#endif
            var componentId = ComponentTools.GetComponentId<T>();

            return false;
//             if (componentId >= _state.SharedStorages.Length)
//             {
// #if DEBUG
//                 throw new Exception("out of ComponentStorages");
// #else
//                 return false;
// #endif
//             }
//
//             var storage = (ComponentsSharedStorage<T>)_state.SharedStorages[componentId];
//             return storage?.Entities.Contains(entityId) ?? false;
        }

        public bool HasStaticComponent<T>(int entityId) where T : struct, IStaticComponent
        {
#if DEBUG
            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
#endif
            var componentId = ComponentTools.GetComponentId<T>();

            return false;
//             if (componentId >= _state.StaticStorages.Length)
//             {
// #if DEBUG
//                 throw new Exception("out of ComponentStorages");
// #else
//                 return false;
// #endif
//             }
//
//             var storage = (ComponentsStaticStorage<T>)_state.StaticStorages[componentId];
//             return storage != null;
        }

        public unsafe ref T GetComponent<T>(int entityId) where T : unmanaged
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
            // var storage = (ComponentsStorage<T>)_state.Storages[componentId];
            return ref _state.Components.Read<T>(componentId, entityId);
        }

        public ref T GetSharedComponent<T>(int entityId) where T : struct, ISharedComponent
        {
#if DEBUG
            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
            if (!HasSharedComponent<T>(entityId))
                throw new Exception($"Entity {entityId} has not component {typeof(T)}");
#endif
            var componentId = ComponentTools.GetComponentId<T>();

            throw new NotImplementedException();
            // var sharedStorage = (ComponentsSharedStorage<T>)_state.SharedStorages[componentId];
            // return ref sharedStorage.Data;
        }

        public ref T GetStaticComponent<T>(int entityId) where T : struct, IStaticComponent
        {
#if DEBUG
            if (!IsAlive(entityId)) throw new Exception($"Entity {entityId} is not alive!");
#endif
            var componentId = ComponentTools.GetComponentId<T>();
            throw new NotImplementedException();

            // var sharedStorage = (ComponentsStaticStorage<T>)_state.StaticStorages[componentId];
            // return ref sharedStorage.Data;
        }

        public readonly bool IsAlive(int entityId) => _state.Entities[entityId].IsAlive;

        public void DestroyEntity(int entityId)
        {
            _lockIndex.Lock();
            {
                var components = GetComponents(entityId);
                components.Clear();

                var entity = GetEntityById(entityId);

                _state.EntityComponents.Remove(entityId);
                _state.Entities[entityId] = default;

                ReturnComponentsInstance(components);

                _entitiesPool.Enqueue(entity);
            }
            _lockIndex.Unlock();
        }

        private UnsafeSparseSet<int> GetComponentsInstance()
        {
            if (_pool.Count == 0)
            {
                return new UnsafeSparseSet<int>
                (
                    ECSSettings.ComponentsDenseCapacity,
                    ECSSettings.ComponentsSparseCapacity
                );
            }

            return _pool.Dequeue();
        }

        private void ReturnComponentsInstance(UnsafeSparseSet<int> instance)
        {
            instance.Clear();
            _pool.Enqueue(instance);
        }

        #region STORAGES

        // private ComponentsStorage<T> GetStorage<T>(int componentId) where T : unmanaged
        // {
        //     var storage = (ComponentsStorage<T>)_state.Storages[componentId];
        //
        //     if (storage != null) return storage;
        //
        //     var newInstance = new ComponentsStorage<T>
        //     (
        //         ECSSettings.ComponentsDenseCapacity,
        //         ECSSettings.ComponentsSparseCapacity
        //     );
        //
        //     _state.Storages[componentId] = newInstance;
        //     storage = newInstance;
        //
        //     return storage;
        // }

        private ComponentsSharedStorage<T> GetSharedStorage<T>(int componentId)
        {
            throw new NotImplementedException();

            // var storage = (ComponentsSharedStorage<T>)_state.SharedStorages[componentId];
            //
            // if (storage != null) return storage;
            //
            // var newInstance = new ComponentsSharedStorage<T>
            // (
            //     new NativeParallelHashSet<int>(30, AllocatorManager.Persistent)
            // );
            //
            // _state.SharedStorages[componentId] = newInstance;
            // storage = newInstance;
            //
            // return storage;
        }

        private ComponentsStaticStorage<T> GetStaticStorage<T>(int componentId)
        {
            throw new NotImplementedException();

            // var storage = (ComponentsStaticStorage<T>)_state.SharedStorages[componentId];
            //
            // if (storage != null) return storage;
            //
            // var newInstance = new ComponentsStaticStorage<T>();
            //
            // _state.StaticStorages[componentId] = newInstance;
            // storage = newInstance;
            //
            // return storage;
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

        private bool IsSharedComponent<T>(int componentId) => false /*_state.SharedStorages[componentId] != null*/;

        public void Dispose()
        {
            while (_pool.Count > 0)
            {
                _pool.Dequeue().Dispose();
            }

            _pool.Dispose();

            _entitiesPool.Dispose();
        }
    }
}