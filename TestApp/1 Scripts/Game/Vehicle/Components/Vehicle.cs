using DesertImage.Collections;
using DesertImage.ECS;
using UnityEngine;

namespace Game.Vehicle
{
    public struct Vehicle
    {
        public BufferList<Entity> Wheels;
        public BufferList<Entity> WheelAxis;
        public float MaxWheelsAngularVelocity;
        public ObjectReference<Transform> Body;
    }
}