using DesertImage.ECS;
using UnityEngine;

namespace Entities
{
    public abstract class EntityExtension : MonoBehaviour
    {
        [SerializeField] private EntityWrapper wrapper;

        protected Entity Entity => wrapper.Entity;

        protected virtual void OnValidate()
        {
            if (wrapper) return;
            wrapper = GetComponent<EntityWrapper>();
        }
    }
}