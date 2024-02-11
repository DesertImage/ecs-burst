using System;
using Unity.Collections;

namespace DesertImage.ECS
{
    public unsafe struct Worlds
    {
        private unsafe struct WorldWrapper
        {
            public World Value;
        }

        private static readonly UnsafeArray<WorldWrapper> _worlds = new UnsafeArray<WorldWrapper>(20, Allocator.Persistent);

        private static int _idCounter = -1;

        public static ref readonly World Initialize()
        {
            var world = new World(++_idCounter);

            if (_worlds.Length <= _idCounter)
            {
#if DEBUG
                throw new Exception("out of worlds capacity");
#endif
            }

            _worlds.Set(_idCounter, new WorldWrapper { Value = world });

            return ref _worlds.Get(_idCounter).Value;
        }

        public static ref readonly World GetCurrent() => ref Get(0);
        public static ref readonly World Get(int id) => ref _worlds.Get(id).Value;
    }
}