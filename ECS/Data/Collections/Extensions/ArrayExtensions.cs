namespace DesertImage.Collections
{
    public static class ArrayExtensions
    {
        public static void ShiftLeft<T>(this ref UnsafeArray<T> array, int startIndex) where T : unmanaged
        {
            for (var i = startIndex; i < array.Length - 1; i++)
            {
                array[i] = array[i + 1];
            }
        }
    }
}