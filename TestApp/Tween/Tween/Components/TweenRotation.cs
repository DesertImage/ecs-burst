using Unity.Mathematics;

namespace Game.Tween
{
    public struct TweenRotation
    {
        public quaternion Start;
        public quaternion End;
        
        public float Time;
        public float ElapsedTime;
        
        public EaseType Ease;
    }
}