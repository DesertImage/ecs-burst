using UnityEngine;

namespace DesertImage.ECS
{
    public abstract class MonoEntityLinkable : MonoBehaviour, IEntityLinkable
    {
        public abstract void Link(Entity entity);
    }
}