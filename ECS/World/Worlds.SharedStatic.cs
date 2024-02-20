using System;
using DesertImage.Collections;
using Unity.Burst;

namespace DesertImage.ECS
{
    public unsafe partial struct Worlds
    {
        private partial struct WorldsIds
        {
            public static readonly SharedStatic<UnsafeQueue<byte>> FreeIds =
                SharedStatic<UnsafeQueue<byte>>.GetOrCreate<WorldsIds>();
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