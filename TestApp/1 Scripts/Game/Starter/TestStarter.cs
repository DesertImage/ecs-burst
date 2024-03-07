using DesertImage.Assets;
using DesertImage.ECS;
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

            const int cardsCount = 5;

            for (var i = 0; i < cardsCount; i++)
            {
                var entity = World.GetNewEntity();

                entity.ReplaceStatic
                (
                    new Hand
                    {
                        Count = cardsCount,
                        CardWidth = 1f,
                        Spacing = 1.5f
                    }
                );

                entity.InstantiateView(0);

                entity.Replace<OriginPosition>();
                entity.Replace<LocalPosition>();

                entity.Replace(new Parent { Value = parent.transform });
                entity.Replace(new HandCard { OrderPosition = i });

                entity.Replace<HandCardAlign>();
            }

            World.Add<ParentToOriginPositionSystem>(ExecutionType.MainThread);
            World.Add<EntityToTransformSystem>(ExecutionType.MainThread);

            World.Add<LocalPositionSystem>();
            World.Add<HandCardAlignSystem>();
        }
    }
}