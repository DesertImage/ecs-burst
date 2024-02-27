using DesertImage.Assets;
using UnityEngine;

namespace Modules.Assets.Spawning
{
    [CreateAssetMenu(menuName = "Test/Objects")]
    public class ObjectLibrary : ScriptableObject, ILibrary<uint, GameObject>
    {
        public void Register(uint id, GameObject item)
        {
            throw new System.NotImplementedException();
        }

        public GameObject Get(uint id)
        {
            throw new System.NotImplementedException();
        }
    }
}