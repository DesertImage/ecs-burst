using Unity.Collections;

namespace DesertImage.Collections
{
    public static class QueueExtensions
    {
        public static UnsafeArray<T> ToUnsafeArray<T>(this ref UnsafeQueue<T> data, Allocator allocator) where T : unmanaged
        {
            var array = new UnsafeArray<T>(data.Count, allocator);

            for (var i = 0; i < data.Count; i++)
            {
                array[i] = data.Dequeue();
            }

            return array;
        }
    }
}