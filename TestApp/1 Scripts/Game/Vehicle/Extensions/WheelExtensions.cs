using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Game.Vehicle
{
    public static class WheelExtensions
    {
        private const float RadToRpmMultiplier = 60 / math.PI2;
        private const float RpmToRadMultiplier = math.PI2 * .01666666f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LinearToAngular(this float distance, float radius) => distance / radius;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AngularToLinear(this float distance, float radius) => distance * radius;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float TorqueToForce(this float torque, float radius) => torque / radius;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ForceToTorque(this float force, float radius) => force * radius;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RpmToRadiansPerSecond(this float rpm) => rpm * RpmToRadMultiplier;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RadiansPerSecondToRpm(this float radians) => radians * RadToRpmMultiplier;
    }
}