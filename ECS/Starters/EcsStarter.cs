using UnityEngine;

namespace DesertImage.ECS
{
    public abstract class EcsStarter : MonoBehaviour
    {
        protected World World;

        protected virtual void Awake()
        {
            World = Worlds.Create();
            Initialize();
        }

        protected virtual void Initialize()
        {
            InitComponents();
            InitSystems();
        }

        protected virtual void OnDestroy() => World.Dispose();

        private void Update() => World.Tick(Time.deltaTime);
        private void FixedUpdate() => World.PhysicTick(Time.fixedDeltaTime);

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