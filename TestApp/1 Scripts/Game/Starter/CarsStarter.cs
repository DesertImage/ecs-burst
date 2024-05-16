using DesertImage.ECS;
using Game.Physics;
using Game.Vehicle;
using UnityEngine;

namespace Game
{
    public class CarsStarter : EcsStarter
    {
        [SerializeField] private EntityView[] wheelViews;
        [SerializeField] private EntityView vehicleView;
        [SerializeField] private EntityView decorView;

        protected override void InitSystems()
        {
            for (var i = 0; i < wheelViews.Length; i++)
            {
                var entity = World.GetNewEntity();

                var view = wheelViews[i];

                entity.Replace(new View { Value = view });
                entity.Replace(new Wheel());
                entity.Replace<WheelVelocity>();

                view.Initialize(entity);
            }

            World.Add<TransformToEntitySystem>(ExecutionOrder.EarlyMainThread);
            
            
            vehicleView.Initialize(World.GetNewEntity());
            decorView.Initialize(World.GetNewEntity());

            World.AddFeature<FakePhysicalObjectFeature>();
            World.AddFeature<VehicleFeature>();

            // World.Add<EntityToTransformSystem>(ExecutionOrder.LateMainThread);
        }
    }
}