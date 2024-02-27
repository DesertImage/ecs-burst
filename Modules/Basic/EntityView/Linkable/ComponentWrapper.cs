using UnityEngine;

namespace DesertImage.ECS
{
    public class ComponentWrapper<T> : MonoEntityLinkable where T : unmanaged
    {
        [SerializeField] protected T Data;

        public override void Link(Entity entity)
        {
            OnDataUpdate(ref Data);
            entity.Replace(Data);
        }

        protected virtual void OnDataUpdate(ref T data)
        {
        }
    }
}