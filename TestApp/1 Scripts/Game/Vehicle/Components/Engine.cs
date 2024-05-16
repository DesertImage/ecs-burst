namespace Game.Vehicle
{
    public struct Engine
    {
        public Curve TorqueCurve;

        public float IdleRpm;
        public float MaxRpm;
        public float Rpm;
        public float RelativeRpm;

        public float Torque;

        public float AngularVelocity;

        public float Intertia;
        public float BackTorque;
    }
}