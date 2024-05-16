using DesertImage.Assets;
using DesertImage.ECS;
using Game.DragAndDrop;
using Game.Input;
using Game.Tween;
using UnityEngine;
using Camera = UnityEngine.Camera;

namespace Game
{
    public class CardgeonStarter : EcsStarter
    {
        protected override void InitModules()
        {
            AddModule(new SpawnManager());
            base.InitModules();
        }

        protected override void InitSystems()
        {
            var parent = new GameObject("Hand")
            {
                transform =
                {
                    position = new Vector3(0f, -2.55f, -2f)
                }
            };

            const int cardsCount = 10;

            for (var i = 0; i < cardsCount; i++)
            {
                var entity = World.GetNewEntity();

                entity.ReplaceStatic(new Hand { Count = cardsCount, });
                entity.ReplaceStatic(new MousePosition());
                entity.ReplaceStatic(new MouseWorldPosition { ZOffset = 10f });
                entity.ReplaceStatic(new MouseRelativePosition());
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
            World.Add<MouseWorldPositionSystem>(ExecutionOrder.EarlyMainThread);
            World.Add<MouseRelativePositionSystem>(ExecutionOrder.EarlyMainThread);

            World.AddFeature<TweenLocalPositionFeature>();
            World.AddFeature<TweenLocalRotationFeature>();
            World.AddFeature<TweenScaleFeature>();
            World.AddFeature<HoverPreviewFeature>();

            World.Add<HandCardPreviewSystem>();
            World.Add<HandCardUnPreviewSystem>();

            World.AddFeature<OriginsFeature>();
            World.AddFeature<DragAndDropFeature>();

            World.Add<HandCardDragSystem>();
            World.Add<HandCardPlaySystem>();
            World.Add<HandRemoveCardSystem>();
            World.Add<HandCardReturnToHandSystem>();
            World.Add<HandCardAlignSystem>();

            World.Add<EntityToTransformSystem>(ExecutionOrder.LateMainThread);

            World.AddRemoveComponentSystem<HandCardAlignTag>();
            World.AddRemoveComponentSystem<HandRemoveCard>();
            World.AddRemoveComponentSystem<PlayedTag>();
        }
    }
}