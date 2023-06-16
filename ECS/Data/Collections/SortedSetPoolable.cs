using System.Collections.Generic;

namespace DesertImage
{
    public class SortedSetPoolable<T> : SortedSet<T>, IPoolable
    {
        public void OnCreate()
        {
        }

        public void ReturnToPool() => Clear();
    }
}