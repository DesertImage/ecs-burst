namespace DesertImage.ECS
{
    public static unsafe class MemoryExtensions
    {
        public static T* Allocate<T>(this T instance) where T : unmanaged => MemoryUtility.Allocate(instance);
    }
}