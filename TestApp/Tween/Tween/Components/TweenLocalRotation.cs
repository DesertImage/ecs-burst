using Unity.Mathematics;

namespace Game.Tween
{
    public struct TweenLocalRotation
    {
        public quaternion Start;
        public quaternion End;
        
        public float Time;
        public float ElapsedTime;
        
        public EaseType Ease;
    }
}