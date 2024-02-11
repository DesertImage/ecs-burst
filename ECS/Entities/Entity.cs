using System;
using System.Diagnostics;

namespace DesertImage.ECS
{
    [Serializable]
    [DebuggerTypeProxy(typeof(EntityDebugView))]
    public struct Entity
    {
        public readonly int Id;
        public bool IsAlive;

        public Entity(int id)
        {
            Id = id;
            IsAlive = true;
        }
    }
}