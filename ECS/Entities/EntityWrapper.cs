using UnityEngine;

namespace DesertImage.ECS
{
    public unsafe class EntityWrapper : MonoBehaviour
    {
        public Entity Entity { get; private set; }

        private IEntityLinkable[] _entityLinkables;

        public void OnCreate()
        {
            Entity = Entities.GetNew(Worlds.Get(0));

            _entityLinkables ??= GetComponents<IEntityLinkable>();
            foreach (var linkable in _entityLinkables)
            {
                linkable.Link(Entity);
            }
        }

        public void ReturnToPool() => Entity.Destroy();

        public static explicit operator Entity(EntityWrapper wrapper) => wrapper.Entity;
    }
}