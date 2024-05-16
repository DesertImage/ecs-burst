using DesertImage.ECS;

namespace Game.Vehicle
{
    public struct WheelAxis
    {
        public float Torque;
        public float BrakeTorque;
        
        public float DriveRatio;
        public float BrakeRatio;
        
        public Entity Left;
        public Entity Right;
    }
}