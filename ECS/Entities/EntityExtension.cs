using DesertImage.ECS;
using UnityEngine;

namespace Entities
{
    public abstract class EntityExtension : MonoBehaviour
    {
        [SerializeField] protected EntityMono entity;

        protected virtual void OnValidate()
        {
            if (entity) return;
            entity = GetComponent<EntityMono>();
        }
    }
}