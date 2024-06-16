using DesertImage.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public unsafe struct World
    {
        private const int CollectionsBufferSize = 2048;
        private const int ComponentsCapacity = 512;
        private const int EntitiesCapacity = 1024;

        public byte Id;

        [NativeDisableUnsafePtrRestriction] public readonly World* Ptr;

        [NativeDisableUnsafePtrRestriction] public readonly WorldState* State;
        [NativeDisableUnsafePtrRestriction] public readonly SystemsState* SystemsState;

        //TODO: refactor
        private readonly ObjectReference<ModuleProvider> _moduleProvider;

        public World(byte id, World* ptr)
        {
            Id = id;
            Ptr = ptr;

            State = MemoryUtility.AllocateInstance(new WorldState(ComponentsCapacity, EntitiesCapacity));

            _moduleProvider = default;

            SystemsState = MemoryUtility.AllocateInstance(new SystemsState(100));
            SystemsState->Context.World = this;
        }

        public World(byte id, World* ptr, ModuleProvider moduleProvider)
        {
            Id = id;
            Ptr = ptr;

            State = MemoryUtility.AllocateInstance(new WorldState(ComponentsCapacity, EntitiesCapacity));

            _moduleProvider = moduleProvider;

            SystemsState = MemoryUtility.AllocateInstance(new SystemsState(100));
            SystemsState->Context.World = this;
        }

        public World(byte id, World* ptr, int componentsCapacity, int entitiesCapacity)
        {
            Id = id;
            Ptr = ptr;

            State = MemoryUtility.AllocateInstance(new WorldState(componentsCapacity, entitiesCapacity));

            _moduleProvider = default;

            SystemsState = MemoryUtility.AllocateInstance(new SystemsState(100));
            SystemsState->Context.World = this;
        }

        public readonly T GetModule<T>() => _moduleProvider.Value.Get<T>();

        public void Dispose()
        {
            SystemsState->Dispose();
            State->Dispose();

            Worlds.Destroy(Id);

            MemoryUtility.Free(State);
            MemoryUtility.Free(SystemsState);

            Id = 0;
        }
    }
}