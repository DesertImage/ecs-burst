using DesertImage.Assets;
using DesertImage.ECS;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectsLibrary", menuName = "DesertImage/Assets libraries/Prefabs")]
public class PrefabsLibrary : AbstractScriptableLibrary<uint, MonoBehaviourPoolable>, IAwake
{
    public void OnAwake(in World world)
    {
        var spawnManager = world.GetModule<SpawnManager>();
        foreach (var node in Nodes)
        {
            spawnManager.Register(node.Id, node.Value, 1);
        }
    }
}