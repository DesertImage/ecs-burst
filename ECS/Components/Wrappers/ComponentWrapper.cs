using UnityEngine;

namespace DesertImage.ECS
{
    public class ComponentWrapper<T> : MonoBehaviour, IEntityLinkable where T : struct
    {
        [SerializeField] protected T Data;

        public void Link(Entity entity)
        {
            OnDataUpdate(ref Data);
            entity.Replace(Data);
        }

        protected virtual void OnDataUpdate(ref T data)
        {
        }
    }
}