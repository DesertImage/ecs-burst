using System;
using System.Diagnostics;

namespace DesertImage.ECS
{
    [Serializable]
    [DebuggerTypeProxy(typeof(EntityDebugView))]
    public struct Entity
    {
        public readonly uint Id;
        public readonly uint WorldId;
        
        internal byte IsAliveFlag;

        public Entity(uint id, uint worldId)
        {
            Id = id;
            WorldId = worldId;
            IsAliveFlag = 1;
        }
    }
}