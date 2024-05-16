using System;
using DesertImage.ECS;
using UnityEngine;

namespace Game.Physics
{
    [Serializable]
    public struct Bound
    {
        public float MinX;
        public float MaxX;

        public float MinY;
        public float MaxY;

        public float MinZ;
        public float MaxZ;
    }

    public struct FakePhysicalDecor
    {
        public Bound Bounds;
        public ObjectReference<Rigidbody> Rigidbody;
        public ObjectReference<Transform> Movable;
    }
}