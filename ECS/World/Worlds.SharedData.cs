using System;
using DesertImage.Collections;
using Unity.Burst;

namespace DesertImage.ECS
{
    public unsafe partial struct Worlds
    {
        private partial struct WorldsIds
        {
            public static readonly SharedStatic<UnsafeQueue<uint>> FreeIds =
                SharedStatic<UnsafeQueue<uint>>.GetOrCreate<WorldsIds>();
        }

        public struct WorldsStorage
        {
            public static readonly SharedStatic<UnsafeArray<IntPtr>> Worlds =
                SharedStatic<UnsafeArray<IntPtr>>.GetOrCreate<WorldsStorage>();
        }

        public struct WorldsCounter
        {
            public static readonly SharedStatic<uint> Counter = SharedStatic<uint>.GetOrCreate<WorldsCounter>();
        }
    }
}