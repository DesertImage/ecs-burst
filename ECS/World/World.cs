using DesertImage.Collections;
using Unity.Burst;
using Unity.Collections;

namespace DesertImage.ECS
{
    public unsafe struct World
    {
        public int Id { get; private set; }

        public bool IsAlive { get; private set; }
        public Entity SharedEntity { get; }

        private WorldState State { get; set; }

        private EntitiesManager* EntitiesManager { get; }
        private GroupsManager* GroupsManager { get; }
        private SystemsManager* SystemsManager { get; }

        public World(int id)
        {
            Id = id;

            IsAlive = true;

            State = new WorldState
            (
                new UnsafeArray<Entity>(10, Allocator.Persistent),
                new UnsafeSparseSet<UnsafeSparseSet<int>>
                (
                    ECSSettings.ComponentsEntitiesCapacity,
                    ECSSettings.ComponentsEntitiesCapacity,
                    20,
                    new UnsafeSparseSet<int>
                    (
                        ECSSettings.ComponentsDenseCapacity,
                        ECSSettings.ComponentsDenseCapacity,
                        ECSSettings.ComponentsSparseCapacity,
                        100
                    )
                ),
                ECSSettings.ComponentsSparseCapacity,
                50
            );

            EntitiesManager = MemoryUtility.Allocate(new EntitiesManager(State));
            GroupsManager = MemoryUtility.Allocate(new GroupsManager(EntitiesManager));
            SystemsManager = MemoryUtility.Allocate(new SystemsManager(EntitiesManager, GroupsManager));

            SharedEntity = EntitiesManager->GetNewEntity();
            GroupsManager->OnEntityCreated(SharedEntity.Id);
        }

        #region COMPONENTS

        public void ReplaceComponent<T>(int entityId, in T component) where T : unmanaged
        {
            var hasComponent = HasComponent<T>(entityId);

            EntitiesManager->ReplaceComponent(entityId, component);

            if (hasComponent) return;

            GroupsManager->OnEntityComponentAdded(entityId, ComponentTools.GetComponentId<T>());
        }

        public void ReplaceSharedComponent<T>(int entityId, in T component) where T : unmanaged, ISharedComponent
        {
            var hasComponent = HasComponent<T>(entityId);

            EntitiesManager->ReplaceSharedComponent(entityId, component);

            if (hasComponent) return;

            GroupsManager->OnEntityComponentAdded(entityId, ComponentTools.GetComponentId<T>());
        }

        public void ReplaceStaticComponent<T>(int entityId, in T component) where T : struct, IStaticComponent
        {
            EntitiesManager->ReplaceStaticComponent(entityId, component);
        }

        public void RemoveComponent<T>(int entityId) where T : unmanaged
        {
            EntitiesManager->RemoveComponent<T>(entityId);
            GroupsManager->OnEntityComponentRemoved(entityId, ComponentTools.GetComponentId<T>());
        }

        public void RemoveSharedComponent<T>(int entityId) where T : struct, ISharedComponent
        {
            EntitiesManager->RemoveSharedComponent<T>(entityId);
            GroupsManager->OnEntityComponentRemoved(entityId, ComponentTools.GetComponentId<T>());
        }

        public bool HasComponent<T>(int entityId) where T : unmanaged => EntitiesManager->HasComponent<T>(entityId);

        public bool HasSharedComponent<T>(int entityId) where T : struct, ISharedComponent
        {
            return EntitiesManager->HasSharedComponent<T>(entityId);
        }

        public bool HasStaticComponent<T>(int entityId) where T : struct, IStaticComponent
        {
            return EntitiesManager->HasStaticComponent<T>(entityId);
        }

        public ref T GetComponent<T>(int entityId) where T : unmanaged =>
            ref EntitiesManager->GetComponent<T>(entityId);

        public ref T GetSharedComponent<T>(int entityId) where T : struct, ISharedComponent
        {
            return ref EntitiesManager->GetSharedComponent<T>(entityId);
        }

        public ref T GetStaticComponent<T>(int entityId) where T : struct, IStaticComponent
        {
            return ref EntitiesManager->GetStaticComponent<T>(entityId);
        }

        #endregion

        public void Add<T>() where T : unmanaged, ISystem => SystemsManager->Add<T>();
        public void AddFeature<T>(T feature) where T : unmanaged, IFeature => feature.Link(this);
        public void AddFeature<T>() where T : unmanaged, IFeature => new T().Link(this);

        public Entity GetEntityById(int id) => EntitiesManager->GetEntityById(id);

        public Entity GetNewEntity()
        {
            var newEntity = EntitiesManager->GetNewEntity();
            GroupsManager->OnEntityCreated(newEntity.Id);
            return newEntity;
        }

        public UnsafeSparseSet<int> GetEntityComponents(int id) => EntitiesManager->GetComponents(id);
        public bool IsEntityAlive(int entityId) => EntitiesManager->IsAlive(entityId);
        public void DestroyEntity(int entityId) => EntitiesManager->DestroyEntity(entityId);

        public EntitiesGroup GetGroup(Matcher matcher) => GroupsManager->GetGroup(matcher);

        public void Tick(float deltaTime) => SystemsManager->Tick(deltaTime);
        public void PhysicTick(float deltaTime) => SystemsManager->PhysicTick(deltaTime);

        public void Dispose()
        {
            State.Dispose();

            SystemsManager->Dispose();
            GroupsManager->Dispose();
            EntitiesManager->Dispose();
        }
    }
}