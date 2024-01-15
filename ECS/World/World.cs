using System.Collections.Generic;

namespace DesertImage.ECS
{
    public struct World
    {
        public int Id { get; private set; }

        public Entity SharedEntity { get; }

        private WorldState State { get; set; }

        private EntitiesManager EntitiesManager { get; }
        private GroupsManager GroupsManager { get; }
        private SystemsManager SystemsManager { get; }

        public World(int id)
        {
            Id = id;

            State = new WorldState
            (
                new Dictionary<int, Entity>(),
                new Dictionary<int, SortedSetPoolable<int>>(),
                new ComponentsStorageBase[ECSSettings.ComponentsDenseCapacity],
                new ComponentsStorageBase[ECSSettings.ComponentsDenseCapacity],
                new ComponentsStorageBase[ECSSettings.ComponentsDenseCapacity]
            );

            EntitiesManager = new EntitiesManager(State);
            GroupsManager = new GroupsManager(EntitiesManager);
            SystemsManager = new SystemsManager(EntitiesManager, GroupsManager);

            SharedEntity = EntitiesManager.GetNewEntity();
            GroupsManager.OnEntityCreated(SharedEntity.Id);
        }

        #region COMPONENTS

        public void ReplaceComponent<T>(int entityId, in T component) where T : struct
        {
            var hasComponent = HasComponent<T>(entityId);

            EntitiesManager.ReplaceComponent(entityId, component);

            if (hasComponent) return;

            GroupsManager.OnEntityComponentAdded(entityId, ComponentTools.GetComponentId<T>());
        }

        public void ReplaceSharedComponent<T>(int entityId, in T component) where T : struct, ISharedComponent
        {
            var hasComponent = HasComponent<T>(entityId);

            EntitiesManager.ReplaceSharedComponent(entityId, component);

            if (hasComponent) return;

            GroupsManager.OnEntityComponentAdded(entityId, ComponentTools.GetComponentId<T>());
        }

        public void ReplaceStaticComponent<T>(int entityId, in T component) where T : struct, IStaticComponent
        {
            EntitiesManager.ReplaceStaticComponent(entityId, component);
        }

        public void RemoveComponent<T>(int entityId) where T : struct
        {
            EntitiesManager.RemoveComponent<T>(entityId);
            GroupsManager.OnEntityComponentRemoved(entityId, ComponentTools.GetComponentId<T>());
        }

        public void RemoveSharedComponent<T>(int entityId) where T : struct, ISharedComponent
        {
            EntitiesManager.RemoveSharedComponent<T>(entityId);
            GroupsManager.OnEntityComponentRemoved(entityId, ComponentTools.GetComponentId<T>());
        }

        public bool HasComponent<T>(int entityId) where T : struct => EntitiesManager.HasComponent<T>(entityId);

        public bool HasSharedComponent<T>(int entityId) where T : struct, ISharedComponent
        {
            return EntitiesManager.HasSharedComponent<T>(entityId);
        }

        public bool HasStaticComponent<T>(int entityId) where T : struct, IStaticComponent
        {
            return EntitiesManager.HasStaticComponent<T>(entityId);
        }

        public ref T GetComponent<T>(int entityId) where T : struct => ref EntitiesManager.GetComponent<T>(entityId);

        public ref T GetSharedComponent<T>(int entityId) where T : struct, ISharedComponent
        {
            return ref EntitiesManager.GetSharedComponent<T>(entityId);
        }

        public ref T GetStaticComponent<T>(int entityId) where T : struct, IStaticComponent
        {
            return ref EntitiesManager.GetStaticComponent<T>(entityId);
        }

        #endregion

        public void Add<T>() where T : class, ISystem, new() => SystemsManager.Add<T>();
        public void AddFeature<T>(T feature) where T : IFeature => feature.Link(this);
        public void AddFeature<T>() where T : IFeature, new() => new T().Link(this);

        public Entity GetEntityById(int id) => EntitiesManager.GetEntityById(id);

        public Entity GetNewEntity()
        {
            var newEntity = EntitiesManager.GetNewEntity();
            GroupsManager.OnEntityCreated(newEntity.Id);
            return newEntity;
        }

        public SortedSetPoolable<int> GetEntityComponents(int id) => EntitiesManager.GetComponents(id);
        public bool IsEntityAlive(int entityId) => EntitiesManager.IsAlive(entityId);
        public void DestroyEntity(int entityId) => EntitiesManager.DestroyEntity(entityId);

        public EntitiesGroup GetGroup(Matcher matcher) => GroupsManager.GetGroup(matcher);

        public void Tick(float deltaTime) => SystemsManager.Tick(deltaTime);
        public void PhysicTick(float deltaTime) => SystemsManager.PhysicTick(deltaTime);

        public void Dispose() => SystemsManager.Dispose();
    }
}