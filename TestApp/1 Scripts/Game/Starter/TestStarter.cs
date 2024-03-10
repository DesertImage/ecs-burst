using DesertImage.Assets;
using DesertImage.ECS;
using Game.Tween;
using Unity.Mathematics;
using UnityEngine;

namespace Game
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
            var parent = new GameObject("Hand");

            const int cardsCount = 10;

            for (var i = 0; i < cardsCount; i++)
            {
                var entity = World.GetNewEntity();

                entity.ReplaceStatic
                (
                    new Hand
                    {
                        Count = cardsCount,
                        CardWidth = new half(1f),
                        Spacing = new half(.3f),
                        RotationStep = new half(10f),
                        VerticalOffsetStep = new half(.2f)
                    }
                );

                entity.InstantiateView(0);

                entity.Replace<OriginPosition>();
                entity.Replace<LocalPosition>();
                entity.Replace<OriginRotation>();
                entity.Replace<LocalRotation>();

                entity.Replace(new Parent { Value = parent.transform });
                entity.Replace(new HandCard { OrderPosition = i });

                entity.Replace<HandCardAlign>();
            }

            World.AddFeature<OriginsFeature>();
            World.AddFeature<TweenLocalPositionFeature>();
            World.AddFeature<TweenLocalRotationFeature>();
            
            World.Add<EntityToTransformSystem>(ExecutionType.MainThread);

            World.Add<HandCardAlignSystem>();
            
            World.Add<RemoveComponentSystem<HandCardAlign>>();
        }
    }
}