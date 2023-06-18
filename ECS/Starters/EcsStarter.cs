using UnityEngine;

namespace DesertImage.ECS
{
    public abstract class EcsStarter : MonoBehaviour
    {
        protected IWorld World;

        protected virtual void Awake()
        {
            World = new World();

            Initialize();
        }

        protected virtual void Initialize()
        {
            InitComponents();
            InitSystems();
        }

        protected virtual void OnDestroy() => World.Dispose();

        private void Update() => World.Tick(Time.deltaTime);

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