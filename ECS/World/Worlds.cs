using System;
using DesertImage.Collections;
using Unity.Collections;

namespace DesertImage.ECS
{
    public unsafe partial struct Worlds
    {
        private static byte _idCounter;

        private static bool _isInitialized;

        private static void Initialize()
        {
            if (_isInitialized) return;

            _idCounter = 0;

            WorldsStorage.Worlds.Data = new UnsafeArray<IntPtr>(20, Allocator.Persistent, default);
            WorldsIds.FreeIds.Data = new UnsafeQueue<byte>(20, Allocator.Persistent);

            _isInitialized = true;
        }

        //TODO: Refactor
        public static World Create()
        {
            if (!_isInitialized) Initialize();

            var id = GetNextWorldId();

            WorldsCounter.Counter.Data++;

            var worldPtr = (World*)MemoryUtility.AllocateClear(MemoryUtility.SizeOf<World>(), Allocator.Persistent);

            *worldPtr = new World(id, worldPtr);
            WorldsStorage.Worlds.Data[id] = (IntPtr)worldPtr;

            return *worldPtr;
        }

        public static World Create(ModuleProvider moduleProvider)
        {
            if (!_isInitialized) Initialize();

            var id = GetNextWorldId();

            WorldsCounter.Counter.Data++;

            var worldPtr = (World*)MemoryUtility.AllocateClear(MemoryUtility.SizeOf<World>(), Allocator.Persistent);

            *worldPtr = new World(id, worldPtr, moduleProvider);
            WorldsStorage.Worlds.Data[id] = (IntPtr)worldPtr;

            return *worldPtr;
        }

        public static ref World Get(ushort id) => ref *GetPtr(id);
        internal static World* GetPtr(ushort id) => (World*)WorldsStorage.Worlds.Data[id];

        public static void Destroy(byte id)
        {
            if (WorldsCounter.Counter.Data == 0)
            {
#if DEBUG_MODE
                throw new Exception("Words count is already 0");
#else
                return;
#endif
            }

            var worldPtr = (void*)WorldsStorage.Worlds.Data[id];
#if DEBUG_MODE
            if((IntPtr)worldPtr == IntPtr.Zero) throw new NullReferenceException("Ptr is null");
#endif
            MemoryUtility.Free(worldPtr);
            WorldsStorage.Worlds.Data[id] = IntPtr.Zero;
            WorldsIds.FreeIds.Data.Enqueue(id);

            WorldsCounter.Counter.Data--;

            if (WorldsCounter.Counter.Data > 0) return;

            Dispose();
        }

        private static byte GetNextWorldId() =>
            WorldsIds.FreeIds.Data.Count > 0 ? WorldsIds.FreeIds.Data.Dequeue() : ++_idCounter;

        private static void Dispose()
        {
            _idCounter = 0;
            _isInitialized = false;

            var worlds = WorldsStorage.Worlds.Data;
            for (var i = 0; i < worlds.Length; i++)
            {
                var ptr = worlds[i];
                MemoryUtility.Free((void*)ptr);
            }

            worlds.Dispose();
            WorldsIds.FreeIds.Data.Dispose();
        }
    }
}