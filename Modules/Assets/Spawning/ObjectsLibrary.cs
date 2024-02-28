using DesertImage.Assets;
using DesertImage.ECS;
using UnityEngine;

namespace Modules.Assets.Spawning
{
    [CreateAssetMenu(menuName = "Test/Objects")]
    public class ObjectsLibrary : AbstractScriptableLibrary<uint, MonoBehaviourPoolable>, IAwake
    {
        public void OnAwake(in World world)
        {
            var spawnManager = world.GetModule<SpawnManager>();

            for (var i = 0; i < Nodes.Count; i++)
            {
                var node = Nodes[i];
                spawnManager.Register(node.Id, node.Value, 1);
            }
        }
    }
}