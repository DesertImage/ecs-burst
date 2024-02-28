using DesertImage.Assets;
using UnityEngine;

namespace DesertImage.ECS
{
    public class TestStarter : EcsStarter
    {
        protected override void InitModules()
        {
            AddModule(new SpawnManager());
            base.InitModules();
        }

        protected override void InitSystems()
        {
            var obj = Get<SpawnManager>().Spawn(0);
            Debug.Log($"spawned isntanceId: {obj.GetInstanceID()}");
            obj.transform.localScale = Vector3.zero;
            
            Get<SpawnManager>().Release(obj);
        }
    }
}