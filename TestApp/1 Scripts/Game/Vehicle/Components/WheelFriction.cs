using Unity.Mathematics;

namespace Game.Vehicle
{
    public struct WheelFriction
    {
        public float2 Value;

        public float2 Slips;
        public float CombinedSlip;
        
        public float SlipRatio;
        
        public float SlipAngle;
        public float SlipAnglePeak;
        
        public float GroundGrip;
        
        public Curve FrictionCurve;

        public float CorneringStiffness;
        public float ForwardStiffness;
    }
}