using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public unsafe struct World
    {
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

            State = MemoryUtility.AllocateInstance(new WorldState(512, 1024));
            
            _moduleProvider = default;
            
            SystemsState = MemoryUtility.AllocateInstance(new SystemsState(100));
            SystemsState->Context.World = this;
        }

        public World(byte id, World* ptr, ModuleProvider moduleProvider)
        {
            Id = id;
            Ptr = ptr;

            State = MemoryUtility.AllocateInstance(new WorldState(512, 1024));

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
            Worlds.Destroy(Id);

            State->Dispose();
            SystemsState->Dispose();

            MemoryUtility.Free(State);
            MemoryUtility.Free(SystemsState);

            Id = 0;
        }
    }
}