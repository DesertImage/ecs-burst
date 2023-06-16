using UnityEngine;

namespace DesertImage.ECS
{
    public class EntityMono : MonoBehaviour, IPoolable
    {
        private Entity _entity;

        private IEntityLinkable[] _entityLinkables;

        public void OnCreate()
        {
            _entity = World.Current.GetNewEntity();

            _entityLinkables ??= GetComponents<IEntityLinkable>();
            foreach (var linkable in _entityLinkables)
            {
                linkable.Link(_entity);
            }
        }

        public void ReturnToPool()
        {
            World.Current.DestroyEntity(_entity.Id);
        }
    }
}