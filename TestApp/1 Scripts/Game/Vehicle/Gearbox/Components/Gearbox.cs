using DesertImage.Collections;

namespace Game.Vehicle
{
    public struct Gearbox
    {
        public BufferArray<float> GearRatio;
        public float TotalGearRatio;

        public float MainGear;

        public int Gear;
        public int Max;

        public float Efficiency;

        public float SwitchTime;
    }
}