using System;

namespace DesertImage.ECS
{
    public interface IWorld : IDisposable
    {
        Entity SharedEntity { get; }

        Entity GetEntityById(int id);
        Entity GetNewEntity();
        SortedSetPoolable<int> GetEntityComponents(int id);
        void DestroyEntity(int entityId);

        void ReplaceComponent<T>(int entityId, T component) where T : struct;
        void RemoveComponent<T>(int entityId) where T : struct;
        bool HasComponent<T>(int entityId) where T : struct;
        ref T GetComponent<T>(int entityId) where T : struct;

        public void Add<T>() where T : class, ISystem, new();

        void Tick(float deltaTime);
        
        EntitiesGroup GetGroup(Matcher matcher);
    }

    public sealed class World : IWorld
    {
        public static World Current { get; private set; }

        public Entity SharedEntity { get; }

        private EntitiesManager EntitiesManager { get; }
        private GroupsManager GroupsManager { get; }
        private SystemsManager SystemsManager { get; }

        public World()
        {
            Current = this;

            GroupsManager = new GroupsManager(this);
            EntitiesManager = new EntitiesManager(this);
            SystemsManager = new SystemsManager(this);

            SharedEntity = EntitiesManager.GetNewEntity();
        }

        public void ReplaceComponent<T>(int entityId, T component) where T : struct
        {
            EntitiesManager.ReplaceComponent(entityId, component);
            GroupsManager.OnEntityComponentAdded(entityId, ComponentTools.GetComponentId<T>());
        }

        public void RemoveComponent<T>(int entityId) where T : struct
        {
            EntitiesManager.RemoveComponent<T>(entityId);
            GroupsManager.OnEntityComponentRemoved(entityId, ComponentTools.GetComponentId<T>());
        }

        public bool HasComponent<T>(int entityId) where T : struct => EntitiesManager.HasComponent<T>(entityId);
        public ref T GetComponent<T>(int entityId) where T : struct => ref EntitiesManager.GetComponent<T>(entityId);

        public void Add<T>() where T : class, ISystem, new() => SystemsManager.Add<T>();

        public Entity GetEntityById(int id) => EntitiesManager.GetEntityById(id);

        public Entity GetNewEntity()
        {
            var newEntity = EntitiesManager.GetNewEntity();
            GroupsManager.OnEntityCreated(newEntity.Id);
            return newEntity;
        }

        public SortedSetPoolable<int> GetEntityComponents(int id) => EntitiesManager.GetComponents(id);

        public void DestroyEntity(int entityId) => EntitiesManager.DestroyEntity(entityId);

        public EntitiesGroup GetGroup(Matcher matcher) => GroupsManager.GetGroup(matcher);

        public void Tick(float delta) => SystemsManager.Tick(delta);

        public void Dispose() => SystemsManager.Dispose();
    }
}