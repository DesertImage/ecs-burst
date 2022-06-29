using DesertImage.ECS;
using UnityEngine;

namespace DesertImage.FX
{
    public class EffectBase : MonoBehaviour, IFX, IPoolable
    {
        public virtual void Play()
        {
        }

        public virtual void Stop()
        {
        }

        public virtual void OnCreate()
        {
        }

        public virtual void ReturnToPool()
        {
            Stop();

            Core.Instance.Get<FXService>().ReturnInstance(this);
        }
    }
}