using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public unsafe struct World
    {
        public byte Id;

        [NativeDisableUnsafePtrRestriction] public readonly WorldState* State;
        [NativeDisableUnsafePtrRestriction] public readonly SystemsState* SystemsState;

        //TODO: refactor
        private ObjectReference<ModuleProvider> _moduleProvider;

        public World(byte id)
        {
            Id = id;

            State = MemoryUtility.Allocate(new WorldState(512, 1024));
            SystemsState = MemoryUtility.Allocate(new SystemsState(20));

            _moduleProvider = default;
        }

        public World(byte id, ModuleProvider moduleProvider)
        {
            Id = id;

            State = MemoryUtility.Allocate(new WorldState(512, 1024));
            SystemsState = MemoryUtility.Allocate(new SystemsState(20));

            _moduleProvider = moduleProvider;
        }

        public World(byte id, int componentsCapacity, int entitiesCapacity)
        {
            Id = id;

            State = MemoryUtility.Allocate(new WorldState(componentsCapacity, entitiesCapacity));
            SystemsState = MemoryUtility.Allocate(new SystemsState(20));

            _moduleProvider = default;
        }

        public T GetModule<T>() => _moduleProvider.Value.Get<T>();

        public void Dispose()
        {
            Worlds.Destroy(Id);

            State->Dispose();
            Id = 0;
        }
    }
}