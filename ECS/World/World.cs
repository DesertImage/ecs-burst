using DesertImage.Collections;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public unsafe struct World
    {
        public byte Id;

        [NativeDisableUnsafePtrRestriction] public readonly WorldState* State;
        [NativeDisableUnsafePtrRestriction] public readonly SystemsState* SystemsState;

        private static uint _entityIdCounter;

        public World(byte id)
        {
            Id = id;

            State = MemoryUtility.Allocate(new WorldState(512, 1024));
            SystemsState = MemoryUtility.Allocate
            (
                new SystemsState
                {
                    EarlyMainThreadSystems = new UnsafeList<ExecuteSystemData>(20, Allocator.Persistent),
                    MultiThreadSystems = new UnsafeList<ExecuteSystemData>(20, Allocator.Persistent),
                    LateMainThreadSystems = new UnsafeList<ExecuteSystemData>(20, Allocator.Persistent),
                    SystemsHash = new UnsafeUintSparseSet<uint>(20)
                }
            );
        }

        public void Dispose()
        {
            Worlds.Destroy(Id);

            State->Dispose();
            Id = 0;
        }
    }
}