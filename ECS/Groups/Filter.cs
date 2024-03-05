using DesertImage.Collections;
using Unity.Collections;

namespace DesertImage.ECS
{
    public unsafe struct Filter
    {
        private UnsafeList<uint> _with;
        private UnsafeList<uint> _withSizes;
        private UnsafeList<uint> _none;
        private uint _componentsHash;
        private World* _world;

        public Filter(in World world)
        {
            _world = world.Ptr;

            _with = new UnsafeList<uint>(3, Allocator.Persistent);
            _withSizes = new UnsafeList<uint>(3, Allocator.Persistent);
            _none = new UnsafeList<uint>(1, Allocator.Persistent);
            _componentsHash = 0;
        }

        public static Filter Create(in World world) => new Filter(world);

        public Filter With<T>() where T : unmanaged
        {
            var componentId = ComponentTools.GetComponentId<T>();

            _componentsHash += componentId;
            _with.Add(componentId);
            _withSizes.Add((uint)MemoryUtility.SizeOf<T>());

            return this;
        }

        public Filter None<T>() where T : unmanaged
        {
            var componentId = ComponentTools.GetComponentId<T>();

            _componentsHash -= componentId;
            _none.Add(componentId);

            return this;
        }

        public ref EntitiesGroup Find()
        {
            var sparseSet = _world->State->Groups;
            var count = sparseSet.Count;
            var groups = sparseSet._dense;

            for (var i = 0; i < count; i++)
            {
                var group = groups[i];
                if (group.ComponentsHashCode != _componentsHash) continue;
                if (group._noneOf.Count != _none.Count) continue;
                if (group._with.Count != _with.Count) continue;

                var isNoneEquals = true;

                foreach (var componentId in group._noneOf)
                {
                    if (!_none.Contains(componentId))
                    {
                        isNoneEquals = false;
                        break;
                    }
                }

                if (!isNoneEquals) continue;

                var isWithEquals = true;

                foreach (var componentId in group._with)
                {
                    if (!_with.Contains(componentId))
                    {
                        isWithEquals = false;
                        break;
                    }
                }

                if (!isWithEquals) continue;

                Dispose();
                return ref sparseSet._dense[i];
            }

            ref var newGroup = ref Groups.GetNewGroup(_world);
            for (var i = 0; i < _with.Count; i++)
            {
                var componentId = _with[i];
                newGroup.With(componentId, _withSizes[i]);
                Groups.AddComponentGroup(componentId, newGroup.Id, _world->State);
            }

            for (var i = 0; i < _none.Count; i++)
            {
                newGroup.None(_none[i]);
            }

            Dispose();
            return ref newGroup;
        }

        private void Dispose()
        {
            _with.Dispose();
            _withSizes.Dispose();
            _none.Dispose();
        }
    }
}