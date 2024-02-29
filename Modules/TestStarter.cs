using DesertImage.Assets;
using Game.Tween;
using Unity.Mathematics;
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
            var entity = World.GetNewEntity();

            entity.Replace<Position>();
            entity.Replace
            (
                new TweenPosition
                {
                    Target = new float3(10f),
                    Time = 2f
                }
            );
            World.AddFeature<TweenPositionFeature>();
        }
    }
}