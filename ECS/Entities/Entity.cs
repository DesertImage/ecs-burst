using System;
using System.Diagnostics;

namespace DesertImage.ECS
{
    [Serializable]
    [DebuggerTypeProxy(typeof(EntityDebugView))]
    public unsafe struct Entity : IEquatable<Entity>
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
        
        public Entity(uint id, World world)
                {
                    Id = id;
                    World = world.Ptr;
                    IsAliveFlag = 1;
                }

        public bool Equals(Entity other) => Id == other.Id;
    }
}