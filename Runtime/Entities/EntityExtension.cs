using DesertImage.ECS;
using UnityEngine;

namespace Entities
{
    public interface IEntityExtension
    {
        void Link(IEntity entity);
    }

    public abstract class EntityExtension : MonoBehaviour, IEntityExtension
    {
        [SerializeField] protected EntityMono Entity;

        protected virtual void OnValidate()
        {
            if (Entity != null) return;

            Entity = GetComponent<EntityMono>();
        }

        public abstract void Link(IEntity entity);
    }
}