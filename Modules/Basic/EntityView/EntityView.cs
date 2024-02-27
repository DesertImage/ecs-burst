using UnityEngine;

namespace DesertImage.ECS
{
    public class EntityView : MonoBehaviour
    {
        public Entity Entity { get; private set; }

        [SerializeField] private MonoEntityLinkable[] linkables;

        public void Initialize(in Entity entity)
        {
            Entity = entity;

            entity.Replace(new View { Value = this });
            entity.Replace<Position>();
            entity.Replace<Rotation>();
            entity.Replace<Scale>();

            for (var i = 0; i < linkables.Length; i++)
            {
                linkables[i].Link(entity);
            }
        }

        protected virtual void OnDestroy() => linkables = null;

        private void OnValidate()
        {
            if (linkables == null || linkables.Length > 0) return;
            linkables = GetComponents<MonoEntityLinkable>();
        }

        public static explicit operator Entity(EntityView view) => view.Entity;
    }
}