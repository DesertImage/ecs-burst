using UnityEngine;

namespace DesertImage.ECS
{
    public abstract class EcsStarter : MonoBehaviour
    {
        protected IWorld World;

        protected virtual void Awake()
        {
            World = new World();

            InitSceneEntities();
            InitComponents();
            InitSystems();
        }

        protected virtual void OnDestroy() => World.Dispose();

        private void Update() => World.Tick(Time.deltaTime);

        private static void InitSceneEntities()
        {
            var sceneEntities = FindObjectsByType<EntityMono>(FindObjectsSortMode.None);
            foreach (var entity in sceneEntities)
            {
                entity.OnCreate();
            }
        }

        private void InitComponents()
        {
            var components = GetComponents<IEntityLinkable>();
            var entity = World.SharedEntity;

            foreach (var componentWrapper in components)
            {
                componentWrapper.Link(entity);
            }
        }

        protected abstract void InitSystems();
    }
}