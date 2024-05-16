using DesertImage.ECS;

namespace Game.Vehicle
{
    public struct VehicleFeature : IFeature
    {
        public void Link(World world)
        {
            new GearboxFeature().Link(world);
            
            world.Add<BrakesInputSystem>(ExecutionOrder.EarlyMainThread);
            world.Add<GasInputSystem>(ExecutionOrder.EarlyMainThread);
            world.Add<SteeringInputSystem>(ExecutionOrder.EarlyMainThread);

            world.Add<EngineSystem>();
            world.Add<VehicleToWheelAxisSystem>();

            world.Add<WheelContactSystem>(ExecutionOrder.Physics);
            world.Add<WheelVelocitySystem>(ExecutionOrder.Physics);
            world.Add<WheelSuspensionSystem>(ExecutionOrder.Physics);
            world.Add<VehicleMaxWheelsAngularVelocitySystem>(ExecutionOrder.Physics);
            world.Add<WheelAxisSteeringSystem>(ExecutionOrder.Physics);
            world.Add<WheelAxisTorqueSystem>(ExecutionOrder.Physics);
            world.Add<WheelSteeringSystem>(ExecutionOrder.Physics);
            world.Add<WheelsTorqueSystem>(ExecutionOrder.Physics);
            // world.Add<WheelSimpleFrictionSystem>(ExecutionOrder.Physics);
            // world.Add<WheelAGSFrictionSystem>(ExecutionOrder.Physics);
            // world.Add<WheelFrictionSystem>(ExecutionOrder.Physics);
            // world.Add<WheelNewFrictionSystem>(ExecutionOrder.Physics);
            // world.Add<WheelTestFrictionSystem>(ExecutionOrder.Physics);
            world.Add<WheelCombinedFrictionSystem>(ExecutionOrder.Physics);
            // world.Add<WheelCombinedNewFrictionSystem>(ExecutionOrder.Physics);
            world.Add<WheelBrakeSystem>(ExecutionOrder.Physics);
            world.Add<WheelRotationSystem>(ExecutionOrder.Physics);
            world.Add<WheelVisualizeSystem>(ExecutionOrder.Physics);
            
            world.Add<VehicleBodyShakingSystem>(ExecutionOrder.LateMainThread);
        }
    }
}