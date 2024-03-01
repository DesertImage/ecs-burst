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

        public static World Create()
        {
            if (!_isInitialized) Initialize();

            var id = GetNextWorldId();

            WorldsCounter.Counter.Data++;

            var world = MemoryUtility.Allocate(new World(id));

            WorldsStorage.Worlds.Data[id] = (IntPtr)world;

            return *world;
        }

        public static World Create(ModuleProvider moduleProvider)
        {
            if (!_isInitialized) Initialize();

            var id = GetNextWorldId();

            WorldsCounter.Counter.Data++;

            var world = MemoryUtility.Allocate(new World(id, moduleProvider));

            WorldsStorage.Worlds.Data[id] = (IntPtr)world;

            return *world;
        }

        public static World Get(ushort id) => *GetPtr(id);
        internal static World* GetPtr(ushort id) => (World*)WorldsStorage.Worlds.Data[id];

        public static void Destroy(byte id)
        {
            if (WorldsCounter.Counter.Data == 0)
            {
#if DEBUG_MODE
                throw new Exception("Words count is already 0");
#endif
                return;
            }

            MemoryUtility.Free((void*)WorldsStorage.Worlds.Data[id]);
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

            WorldsStorage.Worlds.Data.Dispose();
            WorldsIds.FreeIds.Data.Dispose();
        }
    }
}