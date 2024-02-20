using System;
using System.Diagnostics;

namespace DesertImage.ECS
{
    [Serializable]
    [DebuggerTypeProxy(typeof(EntityDebugView))]
    public struct Entity
    {
        public readonly uint Id;
        public readonly ushort WorldId;
        
        internal byte IsAliveFlag;

        public Entity(uint id, ushort worldId)
        {
            Id = id;
            WorldId = worldId;
            IsAliveFlag = 1;
        }
    }
}