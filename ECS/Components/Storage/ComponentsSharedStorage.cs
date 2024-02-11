using Unity.Collections;

namespace DesertImage.ECS
{
    public class ComponentsSharedStorage<T> : ComponentsStorageBase
    {
        public T Data;
        public NativeParallelHashSet<int> Entities;

        public ComponentsSharedStorage(NativeParallelHashSet<int> entities)
        {
            Entities = entities;
        }

        public override void Dispose()
        {
            Entities.Dispose();
        }
    }
}