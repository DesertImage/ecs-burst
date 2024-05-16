using DesertImage.Assets;
using DesertImage.ECS;
using Game.Boids;
using Game.Physics;
using UnityEngine;

namespace Game
{
    public class BoidsStarter : EcsStarter
    {
        protected override void InitModules()
        {
            AddModule(new SpawnManager());
            base.InitModules();
        }

        protected override void InitSystems()
        {
            const int cardsCount = 3;

            for (var i = 0; i < cardsCount; i++)
            {
                var entity = World.GetNewEntity();
                var view = entity.InstantiateView(0);

                view.name = $"Entity #{entity.Id}";

                entity.Replace
                (
                    new Position
                    {
                        Value = new Vector3
                        (
                            Random.Range(-1f, 1f),
                            Random.Range(-1f, 1f),
                            0f
                        )
                    }
                );

                entity.Replace(new Neighbours { Values = entity.CreateSparseSetList<uint, Neighbours>() });
                entity.Replace<Cohesion>();
                // entity.Replace<Velocity>();
            }

            World.Add<NeighboursDetectionSystem>();
            World.Add<CohesionSystem>();

            World.Add<VelocitySystem>();

            World.Add<TransformToEntitySystem>(ExecutionOrder.LateMainThread);
            // World.Add<EntityToTransformSystem>(ExecutionOrder.LateMainThread);
        }
    }
}