using System;
using DesertImage.Collections;
using Unity.Collections;

namespace DesertImage.ECS
{
    public unsafe partial struct Worlds
    {
        private static uint _idCounter;

        private static bool _isInitialized;

        private static void Initialize()
        {
            if (_isInitialized) return;

            _idCounter = 0;

            WorldsStorage.Worlds.Data = new UnsafeArray<IntPtr>(20, Allocator.Persistent, default);
            WorldsIds.FreeIds.Data = new UnsafeQueue<uint>(20, Allocator.Persistent);

            _isInitialized = true;
        }

        public static World Create()
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            var id = GetNextWorldId();

            WorldsCounter.Counter.Data++;

            var world = MemoryUtility.Allocate(new World(id));

            WorldsStorage.Worlds.Data[(int)id] = (IntPtr)world;

            return *world;
        }

        public static World Get(uint id) => *GetPtr(id);
        internal static World* GetPtr(uint id) => (World*)WorldsStorage.Worlds.Data[(int)id];

        public static void Destroy(uint id)
        {
            if (WorldsCounter.Counter.Data == 0)
            {
#if DEBUG
                throw new Exception("Words count is already 0");
#endif
                return;
            }

            MemoryUtility.Free((void*)WorldsStorage.Worlds.Data[(int)id]);
            WorldsStorage.Worlds.Data[(int)id] = IntPtr.Zero;
            WorldsIds.FreeIds.Data.Enqueue(id);

            WorldsCounter.Counter.Data--;

            if (WorldsCounter.Counter.Data > 0) return;

            Dispose();
        }

        private static uint GetNextWorldId() =>
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