using System;
using System.Diagnostics;
using ECS.Entities;

namespace DesertImage.ECS
{
    [Serializable]
    [DebuggerTypeProxy(typeof(EntityDebugView))]
    public struct Entity
    {
        public readonly int Id;

        public Entity(int id) => Id = id;
    }
}