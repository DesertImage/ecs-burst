using Unity.Collections;

namespace DesertImage.Collections
{
    public static class StackExtensions
    {
        public static UnsafeArray<T> ToUnsafeArray<T>(this ref UnsafeStack<T> data, Allocator allocator)
            where T : unmanaged
        {
            var array = new UnsafeArray<T>(data.Count, allocator);

            var count = data.Count;
            for (var i = 0; i < count; i++)
            {
                array[i] = data.Pull();
            }

            return array;
        }

        public unsafe static T[] ToArray<T>(this ref UnsafeStack<T> data) where T : unmanaged
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