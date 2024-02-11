using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace DesertImage.ECS
{
    public static class NativeCollectionsExtensions
    {
        public static void AddRange<T>(this NativeParallelHashSet<T> collection, NativeParallelHashSet<T> range)
            where T : unmanaged, IEquatable<T>
        {
            foreach (var element in range)
            {
                collection.Add(element);
            }
        }

        public static void AddRange<T>(this NativeParallelHashSet<T> collection, NativeParallelHashSet<T> range,
            NativeParallelHashSet<T> exclude)
            where T : unmanaged, IEquatable<T>
        {
            foreach (var element in range)
            {
                if (exclude.Contains(element)) continue;
                collection.Add(element);
            }
        }

        public static void AddRange<T>(this UnsafeParallelHashSet<T> collection, UnsafeParallelHashSet<T> range)
            where T : unmanaged, IEquatable<T>
        {
            foreach (var element in range)
            {
                collection.Add(element);
            }
        }

        public static void AddRange<T>(this UnsafeParallelHashSet<T> collection, UnsafeParallelHashSet<T> range,
            UnsafeParallelHashSet<T> exclude)
            where T : unmanaged, IEquatable<T>
        {
            foreach (var element in range)
            {
                if (exclude.Contains(element)) continue;
                collection.Add(element);
            }
        }
    }
}