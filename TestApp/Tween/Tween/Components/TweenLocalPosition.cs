using Unity.Mathematics;

namespace Game.Tween
{
    public struct TweenLocalPosition
    {
        public uint Id;
        
        public float3 Start;
        public float3 End;
        
        public float Time;
        public float ElapsedTime;
        
        public EaseType Ease;
    }
}