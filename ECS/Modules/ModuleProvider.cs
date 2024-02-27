using UnityEngine;

namespace DesertImage.ECS
{
    public abstract class ModuleProvider : MonoBehaviour, IModuleProvider
    {
        public abstract T Get<T>();
    }
}