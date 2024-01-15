namespace DesertImage.ECS
{
    public class ComponentsStorage<T> : ComponentsStorageBase
    {
        public readonly SparseSet<T> Data;

        public ComponentsStorage(int denseCapacity, int sparseCapacity)
        {
            Data = new SparseSet<T>(denseCapacity, sparseCapacity);
        }
    }
}