using UnityEngine;

namespace DesertImage.Assets
{
    public class MonoBehaviourPoolable : MonoBehaviour, IPoolable
    {
        public uint PoolableId { get; private set; }

        public virtual void OnUnpool(uint id)
        {
            PoolableId = id;
            gameObject.SetActive(true);
        }

        public virtual void OnRelease()
        {
            gameObject.SetActive(false);
        }
    }
}