using DesertImage.ECS;
using UnityEngine;

namespace Entities
{
    public abstract class EntityExtension : MonoBehaviour
    {
        [SerializeField] protected EntityWrapper Wrapper;

        protected virtual void OnValidate()
        {
            if (Wrapper) return;
            Wrapper = GetComponent<EntityWrapper>();
        }
    }
}