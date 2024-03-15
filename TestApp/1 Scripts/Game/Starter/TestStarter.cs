using DesertImage.Assets;
using DesertImage.ECS;
using Game.DragAndDrop;
using Game.Input;
using Game.Tween;
using UnityEngine;
using Camera = UnityEngine.Camera;

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

                entity.ReplaceStatic(new Hand { Count = cardsCount, });
                entity.ReplaceStatic(new MousePosition { ZOffset = 10f });
                entity.ReplaceStatic(new Cameras.Camera { Value = Camera.main });

                entity.Replace<OriginPosition>();
                entity.Replace<LocalPosition>();
                entity.Replace<OriginRotation>();
                entity.Replace<LocalRotation>();

                entity.Replace(new Parent { Value = parent.transform });
                entity.Replace(new HandCard { OrderPosition = i });

                entity.Replace<HandCardAlignTag>();

                entity.InstantiateView(0).name = $"Entity #{entity.Id}";
            }

            World.Add<MousePositionSystem>(ExecutionOrder.EarlyMainThread);

            World.AddFeature<TweenLocalPositionFeature>();
            World.AddFeature<TweenLocalRotationFeature>();
            World.AddFeature<TweenScaleFeature>();
            //
            World.AddFeature<HoverPreviewFeature>();
            //
            World.Add<HandCardPreviewSystem>();
            World.Add<HandCardUnPreviewSystem>();
            World.Add<HandCardAlignSystem>();
            //
            World.AddFeature<OriginsFeature>();
            World.AddFeature<DragAndDropFeature>();
            
            World.Add<HandCardDragSystem>();

            World.Add<EntityToTransformSystem>(ExecutionOrder.LateMainThread);

            World.AddRemoveComponentSystem<HandCardAlignTag>();
        }
    }
}