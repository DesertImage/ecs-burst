using DesertImage.ECS;
using UnityEngine;

namespace Game.Vehicle
{
    public struct Wheel
    {
        public float Mass;
        public float Inertia;
        public float Radius;
        public float RadianRotation;
        public float AngularVelocity;

        public ObjectReference<Transform> View;
    }
}