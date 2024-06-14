using Unity.Collections;

namespace DesertImage.Collections
{
    public static class QueueExtensions
    {
        public static UnsafeArray<T> ToUnsafeArray<T>(this ref UnsafeQueue<T> data, Allocator allocator) where T : unmanaged
        {
            var array = new UnsafeArray<T>(data.Count, allocator);

            var count = data.Count;
            for (var i = 0; i < count; i++)
            {
                array[i] = data.Dequeue();
            }

            return array;
        }
        
        public unsafe static T[] ToArray<T>(this ref UnsafeQueue<T> data) where T : unmanaged
        {
            var array = new T[data.Count];

            var count = data.Count;
            for (var i = 0; i < count; i++)
            {
                array[i] = ((T*)data.GetPtr())[i];
            }

            return array;
        }
    }
}