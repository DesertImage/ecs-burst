using System;
using System.Diagnostics;

namespace DesertImage.ECS
{
    [Serializable]
    [DebuggerTypeProxy(typeof(EntityDebugView))]
    public unsafe struct Entity
    {
        public readonly uint Id;
        public readonly World* World;
        
        internal byte IsAliveFlag;

        public Entity(uint id, World* world)
        {
            Id = id;
            World = world;
            IsAliveFlag = 1;
        }
    }
}